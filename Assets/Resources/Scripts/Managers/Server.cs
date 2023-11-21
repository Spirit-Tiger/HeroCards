using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    public int _firstPlayerId;
    public int _secondPlayerId;

    private int clientConnectedCount;
    public int WinnerId;
    public bool GameStarted = false;
    public bool firstTurn = false;

    public GameState State;

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
        /* if (clientId == 0)
         {
             _client1.ClientId = 1;
         }*/

        if (clientId == 1)
        {
            _client1.ClientId = 1;
        }
        else if (clientId == 2)
        {
            _client2.ClientId = 2;
        }

        clientConnectedCount++;

        if (clientConnectedCount == 2)
        {

            ChangeGameState(GameState.SelectOrder);
        }
    }

    private void SetClientCanAct()
    {
        BattleManager.Instance.SetClientCanActClientRpc();
    }

    public void ChangeGameState(GameState newGameState)
    {
        State = newGameState;

        switch (newGameState)
        {
            case GameState.SelectOrder:
                SelectPlayersOrder();
                break;
            case GameState.DrawCards:
                DrawCards();
                break;
            case GameState.FirstPlayerTurn:
                FirstPlayerTurn();
                break;
            case GameState.SecondPlayerTurn:
                SecondPlayerTurn();
                break;
            case GameState.GameEnd:
                GameEnd();
                break;
        }
    }

    private void SelectPlayersOrder()
    {
        List<GameObject> client1Heroes = new List<GameObject>();
        List<GameObject> client2Heroes = new List<GameObject>();

        int client1Init;
        int client2Init;

        foreach (GameObject hero in _client1.ClientDeck)
        {
            if (hero.GetComponent<Card>().cardStats.Class == UnitCard.CardClass.Hero)
            {
                client1Heroes.Add(hero);
            }
        }

        foreach (GameObject hero in _client2.ClientDeck)
        {
            if (hero.GetComponent<Card>().cardStats.Class == UnitCard.CardClass.Hero)
            {
                client2Heroes.Add(hero);
            }
        }

        client1Init = UnityEngine.Random.Range(0, client1Heroes.Count);
        client2Init = UnityEngine.Random.Range(0, client2Heroes.Count);

        if (client1Heroes[client1Init].GetComponent<Card>().cardStats.Initiative > client2Heroes[client2Init].GetComponent<Card>().cardStats.Initiative)
        {
            _firstPlayerId = 1;
            _secondPlayerId = 2;
        }
        else if (client1Heroes[client1Init].GetComponent<Card>().cardStats.Initiative < client2Heroes[client2Init].GetComponent<Card>().cardStats.Initiative)
        {
            _firstPlayerId = 2;
            _secondPlayerId = 1;
        }
        else
        {
            _firstPlayerId = UnityEngine.Random.Range(1, 3);
            if (_firstPlayerId == 1)
            {
                _secondPlayerId = 2;
            }
            else
            {
                _secondPlayerId = 1;
            }
        }

        GameStarted = true;

        ChangeGameState(GameState.DrawCards);
    }

    private void DrawCards()
    {
        BattleManager.Instance.DrawCardsClientRpc();
        StartCoroutine(StartGame());
        
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        ChangeGameState(GameState.FirstPlayerTurn);
    }

    private void FirstPlayerTurn()
    {

        SpawnManager.Instance.TakeCard(_secondPlayerId);
        BattleManager.Instance.TurnsClientRpc(_firstPlayerId);
        ResetUnitActionPoints(_firstPlayerId);
    }

    private void SecondPlayerTurn()
    {
        SpawnManager.Instance.TakeCard(_firstPlayerId);
        BattleManager.Instance.TurnsClientRpc(_secondPlayerId);
        ResetUnitActionPoints(_secondPlayerId);
    }

    private void GameEnd()
    {
        Debug.Log("EndGame");
        UIManager.Instance.GameEndClientRpc(WinnerId);
    }

    private void ResetUnitActionPoints(int playerId)
    {
        ClientDataSO client = null;
        if (playerId == 1)
        {
            client = _client1;
        }
        else if (playerId == 2)
        {
            client = _client2;
        }
        foreach (GameObject unit in client.UnitsOnField)
        {
            if (unit != null)
            {
                unit.GetComponent<UnitActs>().ResetActPointsClientRpc(playerId);
            }

        }

    }

    private void GetClientDeck()
    {
        _client1.ClientDeck = new List<GameObject>(_deck1.playerDeck);
        _client2.ClientDeck = new List<GameObject>(_deck2.playerDeck);
    }

    public ClientDataSO GetClientData(int clientId)
    {
        /*  if (clientId == 0)
          {
              return _client1;
          }*/

        if (clientId == 1)
        {
            return _client1;
        }
        else if (clientId == 2)
        {
            return _client2;
        }
        return null;
    }
}

public enum GameState
{
    SelectOrder,
    DrawCards,
    FirstPlayerTurn,
    SecondPlayerTurn,
    GameEnd

}
