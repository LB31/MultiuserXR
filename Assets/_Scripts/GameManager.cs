using MLAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TextMeshProUGUI DebugText;
    public MainPlatform CurrentPlatform;
    public ARPlanePlacer MakeAppearOnPlane;
    public ulong OwnClientID;
    public NetworkObject OwnClient
    {
        get { return Client;  }
        set
        {
            Client = value;
            // TODO handle all kinds of XR players
            Client.GetComponent<NetworkPlayer>().PreparePlatformSpecificPlayer();

            HandleAllLookAtObjects();
            //MakeAppearOnPlane.enabled = true;
        }
    }
    public NetworkObject Client;

    protected override void Awake()
    {
        base.Awake();
        //MakeAppearOnPlane.enabled = false;

#if UNITY_EDITOR
        CurrentPlatform = MainPlatform.VR_WINDOWS;
#elif MOBILE
        CurrentPlatform = MainPlatform.MOBILE;
#elif OCULUS_ANDROID
        CurrentPlatform = MainPlatform.VR_ANDROID;
#elif OCULUS_WINDOWS || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        CurrentPlatform = MainPlatform.VR_WINDOWS;
#endif
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
