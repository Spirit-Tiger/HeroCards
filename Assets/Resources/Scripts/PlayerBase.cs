using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerBase : NetworkBehaviour,IActions
{
    public TextMeshProUGUI _hpText;
    public TextMeshProUGUI _baseHp;
    public NetworkVariable<int> BaseHpPlayer1 = new NetworkVariable<int>(0);
    public NetworkVariable<int> BaseHpPlayer2 = new NetworkVariable<int>(0);

    private void OnEnable()
    {
        BaseHpPlayer1.OnValueChanged += BaseHpChanged1;
        BaseHpPlayer2.OnValueChanged += BaseHpChanged2;
    }

    private void OnDisable()
    {
        BaseHpPlayer1.OnValueChanged -= BaseHpChanged1;
        BaseHpPlayer2.OnValueChanged -= BaseHpChanged2;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId && IsClient)
        {
            _hpText.gameObject.SetActive(true);
            _baseHp.gameObject.SetActive(true);
            ChangeBaseHpServerRpc(ClientManager.Instance.GetClientData().ClientHp, (int)clientId);
            _baseHp.text = ClientManager.Instance.GetClientData().ClientHp.ToString();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            ChangeBaseHpServerRpc(-1, 1);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ChangeBaseHpServerRpc(-1, 2);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeBaseHpServerRpc(int hp, int clientId)
    {
        if (clientId == 1)
        {
            BaseHpPlayer1.Value += hp;
            BaseDestroyerd(clientId);
        }
        else if (clientId == 2)
        {
            BaseHpPlayer2.Value += hp;
            BaseDestroyerd(clientId);
        }
    }

    private void BaseDestroyerd(int clientId)
    {
        if (BaseHpPlayer1.Value <= 0 || BaseHpPlayer2.Value <= 0 && Server.Instance.GameStarted)
        {
            Server.Instance.WinnerId = 3 - clientId;
            Server.Instance.ChangeGameState(GameState.GameEnd);
        }
    }

    private void BaseHpChanged1(int preVal, int newVal)
    {
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            _baseHp.text = newVal.ToString();
        }
    }
    private void BaseHpChanged2(int preVal, int newVal)
    {
        if (NetworkManager.Singleton.LocalClientId == 2)
        {
            _baseHp.text = newVal.ToString();
        }
    }

    public void ReceiveDamage(int damage)
    {
        ChangeBaseHpServerRpc(-damage, (3 - ClientManager.Instance.GetClientData().ClientId));
    }

    public void ReceiveHeal(int heal)
    {
        throw new System.NotImplementedException();
    }
}
