using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPlayerController : NetworkBehaviour
{
    public GameObject Nose;
    public float ScaleValue = 0.4f;
    public ulong LocalPlayerID;

    public NetworkVariable<Color> MaterialColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariable<float> OwnScale = new NetworkVariable<float>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private Renderer Renderer;

    // This object is now spawned across the network and RPC's can be sent
    public override void NetworkStart()
    {
        Renderer = GetComponent<Renderer>();
        // Color and size assignments happen on server side
        if (IsServer || IsHost)
        {
            MaterialColor.Value = Random.ColorHSV();
            OwnScale.Value = ScaleValue;            
        }

        if(IsOwner)
        {
            Nose.SetActive(false);
        }
            
        Renderer.material.color = MaterialColor.Value;
        transform.localScale = new Vector3(OwnScale.Value, OwnScale.Value, OwnScale.Value);
    }

    public void PrepareARPlayer()
    {
        transform.parent = Camera.main.transform;
        transform.position = Vector3.zero;
    }

    [ClientRpc]
    private void ChangeColorClientRpc()
    {
        Renderer.material.color = Random.ColorHSV();
    }
}
