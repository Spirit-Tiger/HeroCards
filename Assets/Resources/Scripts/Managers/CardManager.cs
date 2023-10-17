using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static CardSO;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance { get; private set; }

    public bool CardSelected = false;
    public GameObject CardDestroyed;

    public GameObject CardPrefab;
    public GameObject NonPrefab;
    public GameObject CardBackPrefab;
    public GameObject HandField;
    public GameObject HandForOpponent;
    public List<GameObject> HandCards;
    public List<GameObject> HandCards2;

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

        HandField = GameObject.Find("HandField");
        HandForOpponent = GameObject.Find("HandForOpponent");
    }

    public void PlaceCards(int clientId)
    {
        if (clientId == (int)NetworkManager.Singleton.LocalClientId)
        {
            SpawnCardsServerRpc(NetworkManager.Singleton.LocalClientId);
            SpawnCardsBacksServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCardsServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
            clientData.ClientHand.Clear();
            for (int i = 0; i < clientData.ClientDeck.Count; i++)
            {
                GameObject card = Instantiate(CardPrefab, HandField.transform.position, Quaternion.identity);

                card.GetComponent<NetworkObject>().Spawn();
                card.transform.SetParent(HandField.transform);
                card.GetComponent<Card>().CardId.Value = i;

                clientData.ClientHand.Add(card);
                card.GetComponent<NetworkObject>().ChangeOwnership(clientId);

                SpawnCardsClientRpc(clientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCardsBacksServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
            clientData.ClientHandBacks.Clear();
            for (int i = 0; i < clientData.ClientDeck.Count; i++)
            {
                GameObject card = Instantiate(CardBackPrefab, HandForOpponent.transform.position, Quaternion.identity);
                card.GetComponent<NetworkObject>().Spawn();
                card.transform.SetParent(HandForOpponent.transform);
                card.GetComponent<Card>().CardId.Value = i;
                card.GetComponent<RectTransform>().localScale = new Vector3(0.35f, 0.35f, 0.35f);

                clientData.ClientHandBacks.Add(card);
                card.GetComponent<NetworkObject>().ChangeOwnership(clientId);

                SpawnCardsBacksClientRpc(clientId);
            }
        }
    }


    [ClientRpc]
    public void SpawnCardsClientRpc(ulong clientId)
    {

        for (int i = 0; i < HandField.transform.childCount; i++)
        {
            Transform childTransform = HandField.transform.GetChild(i);

            if (NetworkManager.Singleton.LocalClientId != childTransform.GetComponent<NetworkObject>().OwnerClientId)
            {
                childTransform.gameObject.SetActive(false);
            }
        }

    }

    [ClientRpc]
    public void SpawnCardsBacksClientRpc(ulong clientId)
    {
        float counter = 0;
        for (int i = 0; i < HandForOpponent.transform.childCount; i++)
        {
            Transform childTransform = HandForOpponent.transform.GetChild(i);

            if (NetworkManager.Singleton.LocalClientId == childTransform.GetComponent<NetworkObject>().OwnerClientId)
            {
                RectTransform childRect = childTransform.GetComponent<RectTransform>();
                childRect.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                float xPos = counter + (childRect.rect.width * .35f);
                childRect.localPosition = new Vector3(xPos, childRect.localPosition.y, childRect.localPosition.z);
                counter = xPos;
                childTransform.gameObject.SetActive(false);
            }
        }
    }
}
