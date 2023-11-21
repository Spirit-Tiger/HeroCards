using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndTurnAction : NetworkBehaviour
{
    public void NextTurn()
    {
        bool canAct = ClientManager.Instance.GetClientData().ClientCanAct;
        if (canAct)
        {
            BattleManager.Instance.ChangeTurnServerRpc();
        }
    }
}
