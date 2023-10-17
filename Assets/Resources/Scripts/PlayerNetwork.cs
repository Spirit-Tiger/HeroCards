using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private Vector3 camPos = new Vector3(6.65f, 11.76f, 15);
    private Vector3 camRot = new Vector3(60f, -180f, 0);
    private Transform _playerCameraTransform;

    private void Awake()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
        Debug.Log("Player");
        
    }

    private void Update()
    {
        if (!IsOwner) return;

    }

    private void Start()
    {
       
        if (IsOwner && OwnerClientId == 2)
        {
            _playerCameraTransform = transform.GetChild(0);
            _playerCameraTransform.position = camPos;
            _playerCameraTransform.localEulerAngles = camRot;
        }

    }
}
