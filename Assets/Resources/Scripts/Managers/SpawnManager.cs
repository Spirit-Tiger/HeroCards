using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public UnitsList Units;

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
    }


    public void SpawnUnit(int clientId, int unitId, Vector3 startPos)
    {
        SpawnUnitServerRpc(clientId, unitId, startPos);
    }

    public void DespawnCard(int clientId, int cardId)
    {
        DespawnCardServerRpc(clientId, cardId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnUnitServerRpc(int clientId, int unitId,Vector3 startPos)
    {
        Debug.Log("Spawned1");
        GameObject Unit = null;
        Unit = Instantiate(Units.AllUnitsList[unitId], Vector3.one, Quaternion.identity);
        Unit.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)clientId);
        Unit.transform.position = startPos;
        BattleManager.Instance.ChangePlaceModeState(true);
        transform.localScale = Vector3.one;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnCardServerRpc(int clientId,int cardId)
    {
        Debug.Log("Despawned");
        ClientDataSO clientData = Server.Instance.GetClientData((int)clientId);
        clientData.ClientHand[cardId].GetComponent<NetworkObject>().Despawn();
        clientData.ClientHandBacks[cardId].GetComponent<NetworkObject>().Despawn();
        clientData.ClientHand[cardId] = null;
        clientData.ClientHandBacks[cardId] = null;
    }
}
