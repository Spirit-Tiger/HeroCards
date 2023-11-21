using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    public static ClientManager Instance { get; private set; }

    [SerializeField]
    private ClientDataSO _client;

    public GameState State;

    [SerializeField]
    private DeckSO _deck;

    public static event Action OnClientDataChanged;

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

        /* if (NetworkManager.Singleton.LocalClientId == 0)
         {
             _client = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client1");
             _deck = Resources.Load<DeckSO>("Scripts/Decks/PlayerOneDeck");

         }*/



    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            gameObject.GetComponent<NetworkObject>().Despawn(gameObject);
        }

        if (clientId == 1)
        {
            _client = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client1");
            _deck = Resources.Load<DeckSO>("Scripts/Decks/PlayerOneDeck");
            _client.ClientDeck = new List<GameObject>(_deck.playerDeck);
            _client.ClientId = (int)clientId;

        }
        else if (clientId == 2)
        {
            _client = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client2");
            _deck = Resources.Load<DeckSO>("Scripts/Decks/PlayerTwoDeck");
            _client.ClientDeck = new List<GameObject>(_deck.playerDeck);
            _client.ClientId = (int)clientId;
        }
    }

    public void DrawCards()
    {
        CardManager.Instance.HandCards = new List<GameObject>(_client.ClientDeck);
        CardManager.Instance.PlaceCards(_client.ClientId);
    }

    public ClientDataSO GetClientData()
    {
        return _client;
    }

    public void SetClientCanAct(bool canAct)
    {
        _client.ClientCanAct = canAct;
        OnClientDataChanged?.Invoke();
    }
}
