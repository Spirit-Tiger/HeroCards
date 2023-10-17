using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClientData", menuName = "Assets/ClientData")]
public class ClientDataSO : ScriptableObject
{
    public int ClientId;
    public List<GameObject> ClientHand = new List<GameObject>();
    public List<GameObject> ClientHandBacks = new List<GameObject>();
    public List<GameObject> ClientDeck = new List<GameObject>();
    public List<GameObject> PlayedCards = new List<GameObject>();
    public List<GameObject> Graveyard = new List<GameObject>();
    public List<GameObject> UnistOnField = new List<GameObject>();
}
