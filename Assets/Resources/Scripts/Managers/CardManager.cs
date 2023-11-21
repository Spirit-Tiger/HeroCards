
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class CardManager : NetworkBehaviour
{
    public static CardManager Instance { get; private set; }

    public bool CardSelected = false;
    public GameObject CardDestroyed;

    public GameObject CardPrefab;
    public GameObject HandFieldPlayer1;
    public GameObject HandFieldPlayer2;
    public List<GameObject> HandCards;
    public List<GameObject> HandCards2;

    private int _startCardCount = 4;
    public int CardIdCounter = 0;

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

        HandFieldPlayer1 = GameObject.Find("HandFieldPlayer1");
        HandFieldPlayer2 = GameObject.Find("HandFieldPlayer2");
    }

    public void PlaceCards(int clientId)
    {
        if (clientId == 1)
        {
            HandFieldPlayer2.SetActive(false);
        }
        if (clientId == 2)
        {
            HandFieldPlayer1.SetActive(false);
        }

        if (clientId == (int)NetworkManager.Singleton.LocalClientId)
        {
            SpawnCardsServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCardsServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            float counter = 0;
            ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
            clientData.ClientHand.Clear();
            Transform field = null;

            if (clientId == 1)
            {
                field = HandFieldPlayer1.transform;
            }
            else if (clientId == 2)
            {
                field = HandFieldPlayer2.transform;
            }


            for (int i = 0; i < _startCardCount; i++)
            {

                GameObject card = Instantiate(clientData.ClientDeck[0], field.position, Quaternion.identity);
                clientData.ClientDeck.RemoveAt(0);
                RectTransform cardRect = card.GetComponent<RectTransform>();
                card.GetComponent<NetworkObject>().Spawn();
                card.transform.SetParent(field);
                card.GetComponent<Card>().CardId.Value = CardIdCounter++;
                card.GetComponent<Card>().OwnerId.Value = (int)clientId;
                clientData.ClientHand.Add(card);

                float xPos = counter + (cardRect.rect.width * .6f);
                cardRect.localPosition = new Vector3(xPos - field.GetComponent<RectTransform>().rect.width * .4f, cardRect.localPosition.y, cardRect.localPosition.z);
                counter = xPos;
            }
        }
    }

    public void SortHand(List<GameObject> hand)
    {
        for (int j = 0; j < hand.Count; j++)
        {
            RectTransform objectRect = hand[j].GetComponent<RectTransform>();
            float positionX = (objectRect.rect.width * .6f * (j + 1) - HandFieldPlayer1.transform.GetComponent<RectTransform>().rect.width * .4f) ;
            objectRect.localPosition = new Vector3(positionX , objectRect.localPosition.y, objectRect.localPosition.z); 
        }

    }
}
