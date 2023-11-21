using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Card : NetworkBehaviour
{
    public HeroCard cardStats;

    [SerializeField]
    private GameObject _unitPrefab;

    public NetworkVariable<int> CardId = new NetworkVariable<int>();
    public NetworkVariable<int> OwnerId = new NetworkVariable<int>();

    private TextMeshProUGUI _name;
    private TextMeshProUGUI _damage;
    private TextMeshProUGUI _hp;
    private TextMeshProUGUI _manaCost;
    private TextMeshProUGUI _class;


    private void Awake()
    {
       

    }

    private void Start()
    {
        _damage = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        _hp = transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        _name = transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        _manaCost = transform.GetChild(3).GetComponentInChildren<TextMeshProUGUI>();
        _class = transform.GetChild(4).GetComponentInChildren<TextMeshProUGUI>();

        _damage.text = cardStats.Damage.ToString();
        _hp.text = cardStats.Hp.ToString();
        _name.text = cardStats.Name;
        _manaCost.text = cardStats.ManaCost.ToString();
        _class.text = cardStats.Class.ToString();
    }
}
