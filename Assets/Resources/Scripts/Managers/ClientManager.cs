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

    [SerializeField] 
    private DeckSO _deck;
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
        if (NetworkManager.Singleton.LocalClientId == 1)
        {
            _client = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client1");
            _deck = Resources.Load<DeckSO>("Scripts/Decks/PlayerOneDeck");
           
        }
        else if (NetworkManager.Singleton.LocalClientId == 2)
        {
            _client = Resources.Load<ClientDataSO>("Scripts/ClientsData/Client2");
            _deck = Resources.Load<DeckSO>("Scripts/Decks/PlayerTwoDeck");
        }
        _client.ClientDeck = _deck.playerDeck;
        _client.ClientId = (int)NetworkManager.Singleton.LocalClientId;

        CardManager.Instance.HandCards = new List<GameObject>(_client.ClientDeck);
        CardManager.Instance.PlaceCards(_client.ClientId);

    }

    public ClientDataSO GetClientData()
    {
        return _client;
    }
}
