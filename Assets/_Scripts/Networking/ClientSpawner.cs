using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class ClientSpawner : NetworkBehaviour
{
    //public GameObject HostPrefab;
    public GameObject ClientAR;
    public GameObject ClientVR;
    public GameObject ClientXR;

    public override void NetworkStart()
    {
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

        // Spawns an object across the network and makes it the player object for the given client
        netObj.SpawnAsPlayerObject(clientID, null, true);

        // Send RPC only to client with clientID
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientID }
            }
        };
        SpawnReadyClientRpc(clientID, clientRpcParams);
    }

    [ClientRpc]
    public void SpawnReadyClientRpc(ulong clientID, ClientRpcParams clientRpcParams = default)
    {
        //// Only for the new player
        //if(clientID == NetworkManager.Singleton.LocalClientId)
        //{
        NetworkObject netObj = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
        GameManager.Instance.OwnClientID = clientID;
        GameManager.Instance.OwnClient = netObj;
        Debug.Log("SpawnReadyClientRpc " + netObj.name);
        //}
    }

}
