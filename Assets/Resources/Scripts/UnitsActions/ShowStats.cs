using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShowStats : NetworkBehaviour
{
    private Camera _mainCamera;

    private GameObject _statsCanvas;
    private Transform _background;

    private TextMeshProUGUI _nameValue;
    private TextMeshProUGUI _damageValue;
    private TextMeshProUGUI _hpValue;
    private TextMeshProUGUI _speedValue;
    private TextMeshProUGUI _rangeValue;

    private string _name;
    private int _damage;
    private int _hp;
    private int _speed;
    private int _range;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _statsCanvas = GameObject.Find("StatsCanvas");
    }

    private void Start()
    {
        
        _background = _statsCanvas.transform.GetChild(0);
        _nameValue = _background.transform.Find("NameValue").GetComponent<TextMeshProUGUI>();
        _damageValue = _background.transform.Find("DamageValue").GetComponent<TextMeshProUGUI>();
        _hpValue = _background.transform.Find("HpValue").GetComponent<TextMeshProUGUI>();
        _speedValue = _background.transform.Find("SpeedValue").GetComponent<TextMeshProUGUI>();
        _rangeValue = _background.transform.Find("RangeValue").GetComponent<TextMeshProUGUI>();

        _name = gameObject.GetComponent<Unit>().UnitStats.Name;
        _damage = gameObject.GetComponent<Unit>().UnitStats.Damage;
        _hp = gameObject.GetComponent<Unit>().UnitStats.Hp;
        _speed = gameObject.GetComponent<Unit>().UnitStats.Speed;
        _range = gameObject.GetComponent<Unit>().UnitStats.Range;
    }

    private void Update()
    {
        if (IsClient && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) )
            {
                if (hit.collider.gameObject == gameObject)
                {
                    _nameValue.text = _name;
                    _damageValue.text = _damage.ToString();
                    _hpValue.text = _hp.ToString();
                    _speedValue.text = _speed.ToString();
                    _rangeValue.text = _range.ToString();

                    _background.gameObject.SetActive(true);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _background.gameObject.SetActive(false);
        }
    }

}
