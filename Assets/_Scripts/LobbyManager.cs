using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public GameObject VRConnector;
    public GameObject ARConnector;

    public MainPlatform CurrentPlatform;

    private void Awake()
    {
        VRConnector.SetActive(false);
        ARConnector.SetActive(false);

        // When platform is set in inspector for testing let it that way
        if (CurrentPlatform == MainPlatform.UNSPECIFIED)
        {
#if MOBILE
        CurrentPlatform = MainPlatform.MOBILE;
#elif OCULUS_ANDROID
        CurrentPlatform = MainPlatform.VR_ANDROID;
#elif OCULUS_WINDOWS || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        CurrentPlatform = MainPlatform.VR_WINDOWS;
#endif
        }

        if (CurrentPlatform == MainPlatform.MOBILE ||
           CurrentPlatform == MainPlatform.DESKTOP ||
           CurrentPlatform == MainPlatform.SERVER)
        {
            ARConnector.SetActive(true);
        }
        else
        {
            VRConnector.SetActive(true);
        }
    }
}
