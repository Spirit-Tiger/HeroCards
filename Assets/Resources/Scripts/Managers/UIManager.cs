using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    public TextMeshProUGUI CurrentMana;
    public TextMeshProUGUI MaxMana;
    public TextMeshProUGUI TurnText;

    private ClientDataSO _clientData;

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

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

    }

    private void OnClientConnected(ulong clientId)
    {
        _clientData = ClientManager.Instance.GetClientData();
        if (IsClient)
        {
            ClientManager.OnClientDataChanged += UpdateClientData;
            BattleManager.Instance.CurrentManaPlayer1.OnValueChanged += SetCurrentManaPlayer1;
            BattleManager.Instance.MaxManaPlayer1.OnValueChanged += SetMaxManaPlayer1;
            BattleManager.Instance.CurrentManaPlayer2.OnValueChanged += SetCurrentManaPlayer2;
            BattleManager.Instance.MaxManaPlayer2.OnValueChanged += SetMaxManaPlayer2;
        }

    }

    private void OnDisable()
    {

        ClientManager.OnClientDataChanged -= UpdateClientData;
        BattleManager.Instance.CurrentManaPlayer1.OnValueChanged -= SetCurrentManaPlayer1;
        BattleManager.Instance.MaxManaPlayer1.OnValueChanged -= SetMaxManaPlayer1;
        BattleManager.Instance.CurrentManaPlayer2.OnValueChanged -= SetCurrentManaPlayer2;
        BattleManager.Instance.MaxManaPlayer2.OnValueChanged -= SetMaxManaPlayer2;


    }

    private void UpdateClientData()
    {
        _clientData = ClientManager.Instance.GetClientData();
    }

    private void SetCurrentManaPlayer1(int prevVal, int newVal)
    {
        if (_clientData.ClientCanAct)
        {
            CurrentMana.text = newVal.ToString();
        }
    }

    private void SetMaxManaPlayer1(int prevVal, int newVal)
    {
        if (_clientData.ClientCanAct)
        {
            MaxMana.text = newVal.ToString();
        }
    }


    private void SetCurrentManaPlayer2(int prevVal, int newVal)
    {
        if (_clientData.ClientCanAct)
        {
            CurrentMana.text = newVal.ToString();
        }
    }

    private void SetMaxManaPlayer2(int prevVal, int newVal)
    {
        if (_clientData.ClientCanAct)
        {
            MaxMana.text = newVal.ToString();
        }
    }

    [ClientRpc]
    public void GameEndClientRpc(int clientId)
    {
        Debug.Log("GameEnd123");
        if(ClientManager.Instance.GetClientData().ClientId == clientId)
        {
            TurnText.gameObject.SetActive(true);
            TurnText.text = "You Won";
        }
        else
        {
            TurnText.gameObject.SetActive(true);
            TurnText.text = "You Lose";
        }
    }
}
