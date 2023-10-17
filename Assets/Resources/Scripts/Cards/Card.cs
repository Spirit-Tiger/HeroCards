using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Card : NetworkBehaviour
{
    public CardSO cardStats;

    [SerializeField]
    private GameObject _unitPrefab;

    public NetworkVariable<int> CardId = new NetworkVariable<int>();

}
