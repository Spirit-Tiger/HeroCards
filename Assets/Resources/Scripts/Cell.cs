using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Cell : NetworkBehaviour
{
    public int cellId;
    public NetworkVariable<int> OccupiedClientId;
    public NetworkVariable<bool> IsEmpty = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int x;
    public int y;
    public List<Cell> Neighbours = new List<Cell>();
    public GameObject Unit;

    private void Start()
    {
    
    }

}
