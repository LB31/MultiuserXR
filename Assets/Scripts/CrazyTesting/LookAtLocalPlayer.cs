using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtLocalPlayer : NetworkBehaviour
{
    public Transform PlayerObject;
    private ulong localPlayerID;

    public override void NetworkStart()
    {
        //NetworkManager.Singleton.OnClientConnectedCallback += PrepareLooking;       
    }

    public void PrepareLooking(ulong obj)
    {
        localPlayerID = NetworkManager.Singleton.LocalClientId;
        if(localPlayerID == obj)
        {
            PlayerObject = NetworkManager.Singleton.ConnectedClients[localPlayerID].PlayerObject.transform;
        }
    }

    void Update()
    {
        if (IsClient && PlayerObject)
            transform.rotation = Quaternion.LookRotation(transform.position - PlayerObject.position);
    }
}
