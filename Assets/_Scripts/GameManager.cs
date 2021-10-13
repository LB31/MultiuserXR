using MLAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TextMeshProUGUI DebugText;

    public Transform WorldCenter;
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
        Debug.Log("UNITY_EDITOR");
        // For debbing we select the inspector selection
        //CurrentPlatform = MainPlatform.VR_WINDOWS;
#elif MOBILE
Debug.Log("MOBILE");
        CurrentPlatform = MainPlatform.MOBILE;
#elif OCULUS_ANDROID
Debug.Log("OCULUS_ANDROID");
        CurrentPlatform = MainPlatform.VR_ANDROID;
#elif OCULUS_WINDOWS || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
Debug.Log("OCULUS_WINDOWS");
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
