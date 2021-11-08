using MLAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TextMeshProUGUI DebugText;

    public Transform WorldCenter;
    public Canvas DrawCanvas;
    public MainPlatform CurrentPlatform;
    public ARPlanePlacer MakeAppearOnPlane;
    public ulong OwnClientID;
    public NetworkObject OwnClient
    {
        get { return Client;  }
        set
        {
            Client = value;
            Client.GetComponent<NetworkPlayer>().PreparePlatformSpecificPlayer();

            HandleAllLookAtObjects();
            //MakeAppearOnPlane.enabled = true;
        }
    }
    public NetworkObject Client;
    public List<LookAtLocalPlayer> AllLookAtObjects = new List<LookAtLocalPlayer>();

    protected override void Awake()
    {
        base.Awake();
        //MakeAppearOnPlane.enabled = false;

#if UNITY_EDITOR
        // For debbing we select the inspector selection
        //CurrentPlatform = MainPlatform.VR_WINDOWS;
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
        foreach (LookAtLocalPlayer item in AllLookAtObjects)
        {
            item.PrepareLooking(OwnClientID);
        }
    }
     
}
