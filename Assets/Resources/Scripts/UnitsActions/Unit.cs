using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public HeroCard UnitStats;
    public Cell PlacedOnCell;
    public bool Placed = false;

    private Transform _hpBarCanvas;
    private TextMeshProUGUI _hpBar;
    private TextMeshProUGUI _dmgBar;

    private Camera _mainCamera;

    public NetworkVariable<int> OwnerId = new NetworkVariable<int>();

    public NetworkVariable<int> currentHealth;
    public NetworkVariable<int> currentDamage;
    public NetworkVariable<int> currentSpeed;
    public NetworkVariable<int> currentRange;
    public NetworkVariable<int> currentActionPoints;

    private void OnEnable()
    {
        currentHealth.OnValueChanged += ChangeHpValue;
        currentDamage.OnValueChanged += ChangeDamageValue;
    }

    public override void OnDestroy()
    {
        currentHealth.OnValueChanged -= ChangeHpValue;
        currentDamage.OnValueChanged -= ChangeDamageValue;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
        _hpBarCanvas = transform.GetChild(0);
        _hpBar = _hpBarCanvas.GetChild(0).GetComponent<TextMeshProUGUI>();
        _dmgBar = _hpBarCanvas.GetChild(1).GetComponent<TextMeshProUGUI>();

    
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetStartStatsServerRpc();
        
    }

    private void LateUpdate()
    {
        if (IsClient && _mainCamera != null)
        {
            _hpBarCanvas.LookAt(transform.position + _mainCamera.transform.forward);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) {
            SetHealthServerRpc(-1);
        }
    }

    public void ChangeHpValue(int prevVal, int newVal)
    {
        _hpBar.text = newVal.ToString();
    }

    public void ChangeDamageValue(int prevVal, int newVal)
    {
        _dmgBar.text = newVal.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetStartStatsServerRpc()
    {
        currentHealth.Value = UnitStats.Hp;
        currentDamage.Value = UnitStats.Damage;
        currentSpeed.Value = UnitStats.Speed;
        currentRange.Value = UnitStats.Range;
        currentActionPoints.Value = UnitStats.ActionPoints;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealthServerRpc(int healthChange)
    {
        currentHealth.Value += healthChange;
        if(currentHealth.Value <= 0)
        {
            DespawnServerRpc();

            PlacedOnCell.IsEmpty.Value = true;
            PlacedOnCell.Unit = null;
            PlacedOnCell.OccupiedClientId.Value = 0;
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetActionPointsServerRpc(int actPoint)
    {
        currentActionPoints.Value += actPoint;
    }


    [ServerRpc(RequireOwnership = false)]
    public void ResetActionPointsServerRpc(int actPoint)
    {
        currentActionPoints.Value = actPoint;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRpc()
    {
       transform.GetComponent<NetworkObject>().Despawn();
    }
}
