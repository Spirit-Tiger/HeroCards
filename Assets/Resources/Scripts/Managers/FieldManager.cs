using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FieldManager : NetworkBehaviour
{
    public static FieldManager Instance { get; private set; }

    [SerializeField]
    private GameObject _cellContainer;

    public List<GameObject> CellsGO = new List<GameObject>(64);
    public List<Cell> Cells = new List<Cell>(64);

    public HashSet<GameObject> tempSet = new HashSet<GameObject>(64);
    public Material MoveMaterial;
    public Material DefaultMaterial;
    public Material AttackMaterial;
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
            CellsGO.Add(_cellContainer.transform.GetChild(i).gameObject);
            Cells.Add(_cellContainer.transform.GetChild(i).GetComponent<Cell>());
        }
       
        

     /*   int counter1 = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Cells[counter1].x = x;
                Cells[counter1].y = y;
                counter1++;

            }
        }

        foreach (var cell in Cells)
        {
            int counter = 0;
            List<Cell> NeighboursTemp = new List<Cell>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Cells[counter].x == cell.x && (Mathf.Abs(Cells[counter].y - cell.y) == 1))
                    {
                        NeighboursTemp.Add(Cells[counter]);
                    }
                    else if (Cells[counter].y == cell.y && (Mathf.Abs(Cells[counter].x - cell.x) == 1))
                    {
                        NeighboursTemp.Add(Cells[counter]);
                    }
                    counter++;
                }

            }
            cell.Neighbours = new List<Cell>(NeighboursTemp);
        }
*/

    }

    [ServerRpc(RequireOwnership = false)]
    public void EmptyCellsServerRpc()
    {
        if (IsServer)
        {
            Debug.Log("Field Cell Is Empty");
            foreach (var cell in Cells)
            {
                cell.IsEmpty.Value = true;
            }
        }
    }
}
