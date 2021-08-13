using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPlayerController : NetworkBehaviour
{

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

    public override void NetworkStart()
    {
        Renderer = GetComponent<Renderer>();
        // Color and size assignments happen on server side
        if (IsServer || IsHost)
        {
            MaterialColor.Value = Random.ColorHSV();
            OwnScale.Value = 0.2f;
        }
            
        Renderer.material.color = MaterialColor.Value;
        transform.localScale = new Vector3(OwnScale.Value, OwnScale.Value, OwnScale.Value);
    }

    public void PrepareARPlayer()
    {
        transform.parent = Camera.main.transform;
        transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    private void ChangeColorClientRpc()
    {
        Renderer.material.color = Random.ColorHSV();
    }
}
