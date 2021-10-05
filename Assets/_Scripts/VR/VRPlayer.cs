using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRPlayer : XREntity
{
    public XRRig XRRig;
    public GameObject Head;
    public GameManagerVR GameManagerVR;

    public ulong LocalPlayerID;


    public override void NetworkStart()
    {
        Renderer = Head.GetComponent<Renderer>();
        ScaleValue = 1;

        base.NetworkStart();

        if (IsOwner)
        {
            ToggleVR(true);
        }
        else
        {
            ToggleVR(false);
        }
    }

    private void ToggleVR(bool activate)
    {
        XRRig.enabled = activate;
        GameManagerVR.enabled = activate;
        Head.SetActive(!activate);
    }

    public void PrepareVRPlayer()
    {

    }

    [ClientRpc]
    private void ChangeColorClientRpc()
    {
        Renderer.material.color = Random.ColorHSV();
    }
}
