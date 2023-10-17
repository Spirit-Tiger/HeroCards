using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Server : NetworkBehaviour
{
    public static Server Instance;

    [SerializeField]
    private ClientDataSO _client1;

    [SerializeField]
    private ClientDataSO _client2;

    [SerializeField] private DeckSO _deck1;
    [SerializeField] private DeckSO _deck2;

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
        _client1 = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client1");
        _client2 = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client2");
        _deck1 = Resources.Load<DeckSO>("Scripts/Decks/PlayerOneDeck");
        _deck2 = Resources.Load<DeckSO>("Scripts/Decks/PlayerTwoDeck");
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        GetClientDeck();
    }

    private void OnClientConnected(ulong clientId)
    {
        if(clientId == 1)
        {
            _client1.ClientId = 1;
        }
        else if (clientId == 2)
        {
            _client2.ClientId = 2;
        }
    }

    private void GetClientDeck()
    {
        _client1.ClientDeck = _deck1.playerDeck;
        _client2.ClientDeck = _deck2.playerDeck;
    }

    public ClientDataSO GetClientData(int clientId)
    {
        if(clientId == 1)
        {
            return _client1;
        }
        else if(clientId == 2)
        {
            return _client2;
        }
        return null;
    }
}
