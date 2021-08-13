using MLAPI;
using MLAPI.Messaging;
using MLAPI.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ClientSpawner : NetworkBehaviour
{
    //public GameObject HostPrefab;
    public GameObject ClientAR;
    public GameObject ClientVR;
    public GameObject ClientXR;

    public override void NetworkStart()
    {
        //if (IsHost)
        //{
        //    OwnPlayer = Instantiate(HostPrefab, new Vector3(5, 0, 0), Quaternion.identity);
        //    OwnPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, null, true);
        //    //GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        //    //capsule.AddComponent<NetworkObject>().Spawn();
        //    //capsule.AddComponent<NetworkTransform>();
        //    GetComponent<ObjectMover>().OwnSceneObject = OwnPlayer;

        //}
        if (IsClient || IsHost)
        {
            SpawnClientServerRpc(NetworkManager.Singleton.LocalClientId);    
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnClientServerRpc(ulong clientID)
    {
        // TODO handle all kinds of XR players
        GameObject ownPlayer = Instantiate(ClientAR, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkObject netObj = ownPlayer.GetComponent<NetworkObject>();
        //Player2.GetComponent<NetworkObject>().SpawnWithOwnership(clientID, null, true);
        netObj.SpawnAsPlayerObject(clientID, null, true);
        //NetObj.DontDestroyWithOwner = true; // does not work this way
        
        SpawnReadyClientRpc(clientID);
    }

    [ClientRpc]
    public void SpawnReadyClientRpc(ulong clientID)
    {
        

        // Only for the new player
        if(clientID == NetworkManager.Singleton.LocalClientId)
        {
            NetworkObject nNetObj = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
            Debug.Log("SpawnReadyClientRpc " + nNetObj.name);
            // TODO handle all kinds of XR players
            nNetObj.GetComponent<ARPlayerController>().PrepareARPlayer();
            // TODO not here
            var allLookers = FindObjectsOfType<LookAtLocalPlayer>();
            foreach (var item in allLookers)
            {
                item.PrepareLooking(clientID);
            }
        }
    }

}
