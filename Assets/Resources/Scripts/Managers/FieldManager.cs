using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public static FieldManager Instance { get; private set; }

    [SerializeField]
    private GameObject _cellContainer;

    private List<GameObject> _cells = new List<GameObject>();
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


        for(var i = 0; i < _cellContainer.transform.childCount; i++)
        {
            _cells.Add(_cellContainer.transform.GetChild(i).gameObject);
        }

    }
}
