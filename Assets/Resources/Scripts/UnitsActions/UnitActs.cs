using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class UnitActs : NetworkBehaviour, IActions
{
    public HashSet<Cell> moveCells = new HashSet<Cell>();
    public HashSet<Cell> attackCells = new HashSet<Cell>();
    public HashSet<Cell> wholeCells = new HashSet<Cell>();
    public HashSet<Cell> activeCells = new HashSet<Cell>();

    private Unit _unit;
    private Camera _mainCamera;

    private Card _card;

    private int _damage;
    private int _hp;
    private int _moveSpeed;
    private int _attackRange;
    private int _actionPoints;

    private bool _actionMode = false;
    private bool _lookMode = false;
    private bool _movedThisTurn = false;
    private bool _attackedThisTurn = false;
    private bool _canAct = false;

    private ClientDataSO _client;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void UnitInfoChanged(int prevVal, int newVal)
    {
        _unit = gameObject.GetComponent<Unit>();
        _damage = _unit.currentDamage.Value;
        _hp = _unit.currentHealth.Value;
        _moveSpeed = _unit.currentSpeed.Value;
        _attackRange = _unit.currentRange.Value;
        _actionPoints = _unit.currentActionPoints.Value;
    }

    private void Start()
    {
        _unit = gameObject.GetComponent<Unit>();
        _damage = _unit.UnitStats.Damage;
        _hp = _unit.UnitStats.Hp;
        _moveSpeed = _unit.UnitStats.Speed;
        _attackRange = _unit.UnitStats.Range;

        if (IsClient)
        {
            _client = ClientManager.Instance.GetClientData();
            _unit.currentActionPoints.OnValueChanged += UnitInfoChanged;
            _unit.currentDamage.OnValueChanged += UnitInfoChanged;
            _unit.currentHealth.OnValueChanged += UnitInfoChanged;
            _unit.currentRange.OnValueChanged += UnitInfoChanged;
            _unit.currentSpeed.OnValueChanged += UnitInfoChanged;
        }

    }
    private void OnDisable()
    {
        _unit.currentActionPoints.OnValueChanged -= UnitInfoChanged;
        _unit.currentDamage.OnValueChanged -= UnitInfoChanged;
        _unit.currentHealth.OnValueChanged -= UnitInfoChanged;
        _unit.currentRange.OnValueChanged -= UnitInfoChanged;
        _unit.currentSpeed.OnValueChanged -= UnitInfoChanged;

    }

    private void Update()
    {


        if (Input.GetKey(KeyCode.LeftControl))
        {
            UnitRangeShow();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            UnitRangeShowOff();
            _lookMode = false;
        }


        if (IsOwner && _client.ClientCanAct)
        {
            if (_actionMode)
            {
                MoveRangeShowOn();
            }

            if (Input.GetMouseButtonDown(1) && _actionMode && BattleManager.Instance.currentUnit == gameObject)
            {
                MoveRangeShowOff();
            }
        }

    }
    private void OnMouseDown()
    {
        if (IsOwner)
        {
            if (BattleManager.Instance.currentUnit == null)
            {
                BattleManager.Instance.currentUnit = gameObject;
            }
            else if (BattleManager.Instance.currentUnit != gameObject)
            {
                BattleManager.Instance.currentUnit.GetComponent<UnitActs>().UnitRangeShowOff();
                BattleManager.Instance.currentUnit = gameObject;
            }

            if (_client.ClientCanAct)
            {
                if (_unit.Placed && BattleManager.Instance.currentUnit == gameObject)
                {
                    _actionMode = true;
                    ActionRange(_unit.PlacedOnCell, FieldManager.Instance.MoveMaterial, true);
                }
            }
        }



    }

    public void ReceiveDamage(int damage)
    {
        Debug.Log($"Receive damage: {damage}");
        _unit.SetHealthServerRpc(-damage);
    }

    public void ReceiveHeal(int heal)
    {

    }

    private void MoveToCell(Cell cell)
    {
        Debug.Log("MoveUnit");
        SpawnManager.Instance.MoveUnit(_client.ClientId, _unit.GetComponent<NetworkObject>().NetworkObjectId, cell.cellId);
        _unit.SetActionPointsServerRpc(-1);

        transform.position = cell.transform.position + Vector3.up * 0.5f;
        MoveRangeShowOff();

        _unit.PlacedOnCell.Unit = null;
        cell.Unit = gameObject;

        _unit.PlacedOnCell = cell;
        _movedThisTurn = true;

        moveCells.Clear();
        attackCells.Clear();
        wholeCells.Clear();
        activeCells.Clear();
    }

    private void ChangeCellColor(HashSet<Cell> targetCells, Material mat)
    {

        foreach (var cell in targetCells)
        {
            cell.gameObject.GetComponent<MeshRenderer>().material = mat;
        }
    }

    private void UnitRangeShow()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.transform == transform)
            {
                UnitRangeShowOn();
                _lookMode = true;
            }
            if (hit.collider.transform != transform && _lookMode)
            {
                UnitRangeShowOff();
                _lookMode = false;
            }
        }
    }

    private void UnitRangeShowOn()
    {
        MoveRange(_unit.PlacedOnCell, FieldManager.Instance.MoveMaterial);
        WholeRange(_unit.PlacedOnCell, FieldManager.Instance.AttackMaterial);
        _lookMode = true;
    }

    private void UnitRangeShowOff()
    {
        MoveRange(_unit.PlacedOnCell, FieldManager.Instance.DefaultMaterial);
        WholeRange(_unit.PlacedOnCell, FieldManager.Instance.DefaultMaterial);
    }

    private void MoveRangeShowOn()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Cell cell;
        Unit unit;
        PlayerBase playerBase;

        if (Physics.Raycast(ray, out hit) && _canAct)
        {
            cell = hit.collider.gameObject.GetComponent<Cell>();
            unit = hit.collider.gameObject.GetComponent<Unit>();
            playerBase = hit.collider.gameObject.GetComponent<PlayerBase>();
            if (Input.GetMouseButtonDown(0) && _actionPoints > 0)
            {
                if (!_movedThisTurn)
                {
                    if (moveCells.Contains(cell) && cell.IsEmpty.Value && cell.tag != "Player"+ (3 - _client.ClientId))
                    {
                        MoveToCell(hit.collider.gameObject.GetComponent<Cell>());
                    }
                }

                if (!_attackedThisTurn)
                {
                    foreach(Cell targetCell in activeCells)
                    {
                        if(targetCell.tag == "Player" + (3 - _client.ClientId) && playerBase != null && !_attackedThisTurn)
                        {
                            playerBase.GetComponent<IActions>().ReceiveDamage(_damage);
                            _attackedThisTurn = true;
                            _unit.SetActionPointsServerRpc(-1);
                            MoveRangeShowOff();
                        }
                    }
                   
                    if (activeCells.Contains(cell) && !cell.IsEmpty.Value && cell.OccupiedClientId.Value != _client.ClientId)
                    {
                        Debug.Log("Attack");
                        cell.Unit.GetComponent<IActions>().ReceiveDamage(_damage);
                        ReceiveDamage(cell.Unit.GetComponent<UnitActs>()._damage);
                        _attackedThisTurn = true;
                        _unit.SetActionPointsServerRpc(-1);
                        MoveRangeShowOff();
                    }
                    if (unit != null && unit.PlacedOnCell.OccupiedClientId.Value != _client.ClientId && !unit.PlacedOnCell.IsEmpty.Value)
                    {
                        Debug.Log("Attack");
                        unit.gameObject.GetComponent<IActions>().ReceiveDamage(_damage);
                        ReceiveDamage(unit.gameObject.GetComponent<UnitActs>()._damage);
                        _attackedThisTurn = true;
                        _unit.SetActionPointsServerRpc(-1);
                        MoveRangeShowOff();
                    }
                }


            }

        }
    }

    private void MoveRangeShowOff()
    {
        _actionMode = false;
        ActionRange(_unit.PlacedOnCell, FieldManager.Instance.DefaultMaterial, false);
    }

    private void ActionRange(Cell cell, Material mat, bool trigger)
    {
        MoveRange(_unit.PlacedOnCell, mat);
        GetRange(cell, wholeCells, _moveSpeed, _attackRange);
        activeCells.AddRange(moveCells);
        foreach (Cell rangeCell in wholeCells)
        {
            if (!rangeCell.IsEmpty.Value && trigger && rangeCell.OccupiedClientId.Value != _client.ClientId)
            {
                rangeCell.gameObject.GetComponent<MeshRenderer>().material = FieldManager.Instance.AttackMaterial;
                activeCells.Add(rangeCell);
            }
            else if (!trigger)
            {
                rangeCell.gameObject.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }

    private void MoveRange(Cell cell, Material mat)
    {
        GetRange(cell, moveCells, _moveSpeed);

        HashSet<Cell> set = new HashSet<Cell>();

        foreach (Cell cellToMove in moveCells)
        {
          if(cellToMove.tag != "Player" + (3 - _client.ClientId))
            {
                set.Add(cellToMove);
            }
        }
        ChangeCellColor(set, mat);

    }

    private void AttackRange(Cell cell, Material mat)
    {
        GetRange(cell, attackCells, _attackRange);
        ChangeCellColor(attackCells, mat);
    }

    private void WholeRange(Cell cell, Material mat)
    {
        GetRange(cell, wholeCells, _moveSpeed, _attackRange);

        IEnumerable<Cell> attackCellsEnumerable = wholeCells;
        IEnumerable<Cell> totalMovesEnumerable = moveCells;

        HashSet<Cell> cells = new HashSet<Cell>(attackCellsEnumerable.Except(totalMovesEnumerable));

        ChangeCellColor(cells, mat);

    }

    private void GetRange(Cell currentCell, HashSet<Cell> cellsSet, int range)
    {
        int depth = 0;
        cellsSet.AddRange(currentCell.Neighbours);

        Recurs(cellsSet, currentCell, range, depth);
        cellsSet.Remove(currentCell);
    }

    private void GetRange(Cell currentCell, HashSet<Cell> cellsSet, int range, int extra)
    {
        int depth = 0;
        cellsSet.AddRange(currentCell.Neighbours);

        Recurs(cellsSet, currentCell, range + extra, depth);
        cellsSet.Remove(currentCell);
    }

    private void Recurs(HashSet<Cell> cellSet, Cell cell, int range, int depth)
    {
        depth++;
        cellSet.AddRange(cell.Neighbours);

        if (range > depth)
        {
            foreach (Cell cell1 in cell.Neighbours)
            {
                cellSet.AddRange(cell1.Neighbours);
                Recurs(cellSet, cell1, range, depth);
            }
        }
    }

    [ClientRpc]
    public void ResetActPointsClientRpc(int ownerId)
    {
        if (_unit.OwnerId.Value == ownerId)
        {
            _movedThisTurn = false;
            _attackedThisTurn = false;
            _canAct = true;
            _unit.ResetActionPointsServerRpc(_unit.UnitStats.ActionPoints);

        }
    
    }
}
