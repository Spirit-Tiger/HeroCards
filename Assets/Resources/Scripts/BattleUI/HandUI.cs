using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject HandBox;

    private Vector3 _upPosition = new Vector3(0, 200f, 0);
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsClient)
        {
            MoveFieldServerRpc();
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveFieldServerRpc()
    {
        HandBox.transform.position = HandBox.transform.position + _upPosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsClient)
        {
            MoveField2ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveField2ServerRpc()
    {
        HandBox.transform.position = HandBox.transform.position - _upPosition;
    }

}
