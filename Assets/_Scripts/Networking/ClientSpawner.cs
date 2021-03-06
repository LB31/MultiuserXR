using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using UnityEngine;

public class ClientSpawner : NetworkBehaviour
{
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
        GameObject ownPlayer = Instantiate(ClientXR, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkObject netObj = ownPlayer.GetComponent<NetworkObject>();

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
        var ownClient = NetworkSpawnManager.GetLocalPlayerObject();
        ownClient.GetComponent<NetworkPlayer>().PreparePlatformSpecificPlayer();
        GameManager.Instance.HandleAllLookAtObjects(clientID);
    }

}
