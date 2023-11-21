using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.XR;
using static CardSO;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public UnitsList Units;

    public GameObject UnitToPlaceOnCell;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public void SpawnUnit(int clientId, int unitId, Vector3 startPos)
    {
        SpawnUnitServerRpc(clientId, unitId, startPos);
    }

    public void PlaceUnit(int cellId)
    {
        PlaceUnitServerRpc(cellId);
    }

    public void MoveUnit(int clientId, ulong objectId, int cellId)
    {
        MoveUnitServerRpc(clientId, objectId, cellId);
    }

    public void DespawnCard(int clientId, int cardId)
    {
        DespawnCardServerRpc(clientId, cardId);
    }


    public void ChangeCellEmptyField(bool isEmpty, int cellId)
    {
        ChangeCellEmptyFieldServerRpc(isEmpty, cellId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnUnitServerRpc(int clientId, int unitId, Vector3 startPos)
    {
        GameObject Unit = null;

        foreach (GameObject unit in Units.AllUnitsList)
        {
            if (unit.GetComponent<Unit>().UnitStats.UnitId == unitId)
            {
                Unit = Instantiate(unit, Vector3.one, Quaternion.identity);
                Unit.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)clientId);
                Unit.GetComponent<Unit>().OwnerId.Value = clientId;
                UnitToPlaceOnCell = Unit;
                ClientDataSO clientData = Server.Instance.GetClientData(clientId);
                clientData.UnitsOnField.Add(Unit);
            }
        }


        Unit.transform.position = startPos;
        BattleManager.Instance.ChangePlaceModeState(true);
        transform.localScale = Vector3.one;
    }


    [ServerRpc(RequireOwnership = false)]
    public void PlaceUnitServerRpc(int cellId)
    {
        Cell targetCell = null;
        targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);
        targetCell.Unit = UnitToPlaceOnCell;
        targetCell.IsEmpty.Value = false;
        targetCell.OccupiedClientId.Value = UnitToPlaceOnCell.GetComponent<Unit>().OwnerId.Value;
        UnitToPlaceOnCell.GetComponent<Unit>().PlacedOnCell = targetCell;
        SetUnitForCellClientRpc(UnitToPlaceOnCell.GetComponent<NetworkObject>().NetworkObjectId, cellId);
        UnitToPlaceOnCell = null;
    }

    [ClientRpc]
    private void SetUnitForCellClientRpc(ulong networkObjectId, int cellId)
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.NetworkObjectId == networkObjectId)
            {
                Cell targetCell = null;
                targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);
                targetCell.Unit = networkObject.gameObject;
                networkObject.gameObject.GetComponent<Unit>().PlacedOnCell = targetCell;

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveUnitServerRpc(int clientId, ulong objectId, int cellId)
    {
        Cell targetCell = null;
        GameObject UnitToMove = null;
        Unit unit = null;
        ClientDataSO clientData = Server.Instance.GetClientData(clientId);

        UnitToMove = clientData.UnitsOnField.Find(unit => unit.GetComponent<NetworkObject>().NetworkObjectId == objectId);
        unit = UnitToMove.GetComponent<Unit>();

        unit.PlacedOnCell.Unit = null;
        unit.PlacedOnCell.IsEmpty.Value = true;
        unit.PlacedOnCell.OccupiedClientId.Value = 0;

        targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);

        targetCell.Unit = UnitToMove;
        targetCell.IsEmpty.Value = false;
        targetCell.OccupiedClientId.Value = UnitToMove.GetComponent<Unit>().OwnerId.Value;

        unit.PlacedOnCell = targetCell;

        MoveUnitClientRpc(UnitToMove.GetComponent<NetworkObject>().NetworkObjectId, cellId);

    }

    [ClientRpc]
    private void MoveUnitClientRpc(ulong networkObjectId, int cellId)
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.NetworkObjectId == networkObjectId)
            {
                Cell targetCell = null;
                targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);

                networkObject.gameObject.GetComponent<Unit>().PlacedOnCell.Unit = null;
                networkObject.gameObject.GetComponent<Unit>().PlacedOnCell = null;

                networkObject.gameObject.GetComponent<Unit>().PlacedOnCell = targetCell;
                networkObject.gameObject.GetComponent<Unit>().PlacedOnCell.Unit = networkObject.gameObject;

            }
        }
    }

    public void TakeCard(int clientId)
    {
        ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
        Transform field = null;

        if (clientData.ClientDeck.Count > 0)
        {
            if (clientId == 1)
            {
                field = CardManager.Instance.HandFieldPlayer1.transform;
            }
            else if (clientId == 2)
            {
                field = CardManager.Instance.HandFieldPlayer2.transform;
            }

            GameObject card = Instantiate(clientData.ClientDeck[0], field.position, Quaternion.identity);
            card.GetComponent<NetworkObject>().Spawn();
            card.transform.SetParent(field);
            card.GetComponent<Card>().CardId.Value = CardManager.Instance.CardIdCounter++;
            card.GetComponent<Card>().OwnerId.Value = (int)clientId;

            clientData.ClientHand.Add(card);
            clientData.ClientDeck.RemoveAt(0);

            CardManager.Instance.SortHand(clientData.ClientHand);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnCardServerRpc(int clientId, int cardId)
    {
        Debug.Log("Despawned");
        ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
        GameObject card = null;
        card = clientData.ClientHand.Find(card => card.GetComponent<Card>().CardId.Value == cardId);
        card.GetComponent<NetworkObject>().Despawn();
        clientData.ClientHand.Remove(card);

        CardManager.Instance.SortHand(clientData.ClientHand);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCellEmptyFieldServerRpc(bool isEmpty, int cellId)
    {
        Cell targetCell = null;
        targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);
        targetCell.IsEmpty.Value = isEmpty;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCellUnitServerRpc(bool isEmpty, int cellId)
    {
        Cell targetCell = null;
        targetCell = FieldManager.Instance.Cells.Find(cell => cell.cellId == cellId);
        targetCell.IsEmpty.Value = isEmpty;
    }

}
