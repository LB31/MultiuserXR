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

    public void PrepareLooking(ulong clientID)
    {
        //localPlayerID = NetworkManager.Singleton.LocalClientId;
        //if(localPlayerID == clientID)
        //{
        //    PlayerObject = NetworkManager.Singleton.ConnectedClients[localPlayerID].PlayerObject.transform;
        //}
        PlayerObject = GameManager.Instance.OwnClient.transform;
    }

    void Update()
    {
        if ((IsClient || IsHost) && PlayerObject != null)
            transform.rotation = Quaternion.LookRotation(transform.position - PlayerObject.position);
    }
}
