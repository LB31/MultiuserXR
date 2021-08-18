using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPlayer : XREntity
{
    public GameObject Nose;
    
    public ulong LocalPlayerID;


    public override void NetworkStart()
    {
        base.NetworkStart();

        if (IsOwner)
        {
            Nose.SetActive(false);
        }
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
