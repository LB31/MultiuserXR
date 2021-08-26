using MLAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;

public class GameManager : Singleton<GameManager>
{
    public TextMeshProUGUI DebugText;

    public ARPlanePlacer MakeAppearOnPlane;
    public ulong OwnClientID;
    public NetworkObject OwnClient
    {
        get { return Client;  }
        set
        {
            Client = value;
            // TODO handle all kinds of XR players
            Client.GetComponent<ARPlayer>().PrepareARPlayer();
            HandleAllLookAtObjects();
            MakeAppearOnPlane.enabled = true;
        }
    }
    public NetworkObject Client;

    protected override void Awake()
    {
        base.Awake();
        MakeAppearOnPlane.enabled = false;
    }

    private void HandleAllLookAtObjects()
    {
        var allLookers = FindObjectsOfType<LookAtLocalPlayer>();
        foreach (var item in allLookers)
        {
            item.PrepareLooking(OwnClientID);
        }
    }
     
}