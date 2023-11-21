using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameObject ServerPrefab;
    public GameObject ClientPrefab;

    public GameObject currentUnit;

    public bool IsDragging = false;
    public bool PlaceMode = false;
    public int PlayerId;
    public DeckSO DeckData;
    public DeckSO DeckData2;
    public List<GameObject> Deck;

    public NetworkVariable<int> CurrentManaPlayer1 = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> MaxManaPlayer1 = new NetworkVariable<int>(3);
    public NetworkVariable<int> CurrentManaPlayer2 = new NetworkVariable<int>(3);
    public NetworkVariable<int> MaxManaPlayer2 = new NetworkVariable<int>(3);

    public static event Action<bool> OnDragStateChanged;
    public static event Action<bool> OnPlaceModeStateChanged;

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

        /*if (PlayerId == 0)
        {
            Deck = DeckData.playerDeck;
        }*/

        if (PlayerId == 1)
        {
            Deck = DeckData.playerDeck;
        }
        else if (PlayerId == 2)
        {
            Deck = DeckData2.playerDeck;
        }

        /*        if (!IsServer)
                {
                    Instantiate(ClientPrefab);
                }*/
        FieldManager.Instance.EmptyCellsServerRpc();
    }

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

    [ClientRpc]
    public void SetClientCanActClientRpc()
    {
        ClientManager.Instance.SetClientCanAct(true);
    }

    [ClientRpc]
    public void DrawCardsClientRpc()
    {
        ClientManager.Instance.DrawCards();
    }

    [ClientRpc]
    public void TurnsClientRpc(int playerId)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)playerId)
        {
            ClientManager.Instance.SetClientCanAct(true);
            UIManager.Instance.TurnText.text = "Your Turn";
            UIManager.Instance.TurnText.gameObject.SetActive(true);

            SetManaServerRpc();
            StartCoroutine(DisableText());

        }
        if (NetworkManager.Singleton.LocalClientId != (ulong)playerId)
        {
            UIManager.Instance.TurnText.text = "Enemy Turn";
            UIManager.Instance.TurnText.gameObject.SetActive(true);
            ClientManager.Instance.SetClientCanAct(false);
            StartCoroutine(DisableText());
        }
    }

    private IEnumerator DisableText()
    {
        yield return new WaitForSeconds(2);
        UIManager.Instance.TurnText.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetManaServerRpc()
    {
        if (Server.Instance.State == GameState.FirstPlayerTurn)
        {
            MaxManaPlayer1.Value++;
            CurrentManaPlayer1.Value = MaxManaPlayer1.Value;
            Debug.Log(CurrentManaPlayer1.Value + "  " + MaxManaPlayer1.Value);
        }
        if (Server.Instance.State == GameState.SecondPlayerTurn)
        {
            MaxManaPlayer2.Value++;
            CurrentManaPlayer2.Value = MaxManaPlayer2.Value;
            Debug.Log(CurrentManaPlayer2.Value + "  " + MaxManaPlayer2.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseManaServerRpc(int manaCost)
    {
        if (Server.Instance.State == GameState.FirstPlayerTurn)
        {
            CurrentManaPlayer1.Value += manaCost;
           
        }
        if (Server.Instance.State == GameState.SecondPlayerTurn)
        {
            CurrentManaPlayer2.Value += manaCost;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeTurnServerRpc()
    {
        if (Server.Instance.State == GameState.FirstPlayerTurn)
        {
            SecondPlayerTurnState();
            SetGameStateClientRpc(GameState.SecondPlayerTurn);
        }
        else if (Server.Instance.State == GameState.SecondPlayerTurn)
        {
            FirstPlayerTurnState();
            SetGameStateClientRpc(GameState.FirstPlayerTurn);
        }
    }

    [ClientRpc]
    public void SetGameStateClientRpc(GameState gameState)
    {
        ClientManager.Instance.State = gameState;
    }

    private void SecondPlayerTurnState()
    {
        Server.Instance.ChangeGameState(GameState.SecondPlayerTurn);
    }

    private void FirstPlayerTurnState()
    {
        Server.Instance.ChangeGameState(GameState.FirstPlayerTurn);
    }
}
