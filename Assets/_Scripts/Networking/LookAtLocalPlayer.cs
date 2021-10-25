using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtLocalPlayer : NetworkBehaviour
{
    public float MaxistanceToLook = 2;
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
        NetworkPlayer np = GameManager.Instance.OwnClient.GetComponent<NetworkPlayer>();
        PlayerObject = np.Head;
    }

    void Update()
    {
        if (!PlayerObject) return;

        float distance = Vector3.Distance(PlayerObject.position, transform.position);
        if ((IsClient || IsHost) && PlayerObject != null && distance > MaxistanceToLook)
        {
            Vector3 dir = transform.position - PlayerObject.position;
            Quaternion lookAtRotation = Quaternion.LookRotation(dir);
            Quaternion lookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, lookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = lookAtRotationOnly_Y;
        }
        //transform.rotation = Quaternion.LookRotation(transform.position - PlayerObject.position);
    }
}
