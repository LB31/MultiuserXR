using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XREntity : NetworkBehaviour
{
    public float ScaleValue = 0.4f;

    protected NetworkVariable<Color> MaterialColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    protected NetworkVariable<float> OwnScale = new NetworkVariable<float>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    protected Renderer Renderer;

    public override void NetworkStart()
    {
        Renderer = GetComponent<Renderer>();
        // Color and size assignments happen on server side
        if (IsServer || IsHost)
        {
            MaterialColor.Value = Random.ColorHSV();
            OwnScale.Value = ScaleValue;
        }

        Renderer.material.color = MaterialColor.Value;
        transform.localScale = new Vector3(OwnScale.Value, OwnScale.Value, OwnScale.Value);
    }
}
