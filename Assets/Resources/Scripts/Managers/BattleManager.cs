using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameObject ServerPrefab;
    public GameObject ClientPrefab;

    public bool IsDragging = false;
    public bool PlaceMode = false;
    public int PlayerId;
    public DeckSO DeckData;
    public DeckSO DeckData2;
    public List<GameObject> Deck;
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> HandBacks = new List<GameObject>();
    public List<GameObject> Hand2 = new List<GameObject>();
    public List<GameObject> HandBacks2 = new List<GameObject>();

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

    public override void OnNetworkSpawn()
    {
        Debug.Log("BattleManagerSpawned");
        if (IsServer)
        {
            Instantiate(ServerPrefab);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        PlayerId = (int)clientId;

        if (PlayerId == 1)
        {
            Deck = DeckData.playerDeck;
        }
        else if (PlayerId == 2)
        {
            Deck = DeckData2.playerDeck;
        }

        if (!IsServer)
        {
            Instantiate(ClientPrefab);
        }
    }

    private void Update()
    {
       
    }



    public static event Action<bool> OnDragStateChanged;
    public static event Action<bool> OnPlaceModeStateChanged;

    public void ChangeDragState(bool dragState)
    {
        IsDragging = dragState;
        OnDragStateChanged?.Invoke(dragState);
    }

    public void ChangePlaceModeState(bool placeModeState)
    {
        PlaceMode = placeModeState;
        OnPlaceModeStateChanged?.Invoke(placeModeState);
    }
}
