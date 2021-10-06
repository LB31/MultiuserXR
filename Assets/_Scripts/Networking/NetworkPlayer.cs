using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform Head;
    public Transform LeftHand;
    public Transform RightHand;

    [Header("VR Components")]
    public GameObject VRPlayer;
    public Transform VRHead;
    public Transform VRLeftHand;
    public Transform VRRightHand;

    [Header("AR Components")]
    public GameObject ARPlayer;
    public Transform ARHead;

    public override void NetworkStart()
    {

    }

    public void PreparePlatformSpecificPlayer()
    {
        if (IsServer && !IsHost) return;

        Debug.Log("PreparePlatformSpecificPlayer " + GameManager.Instance.CurrentPlatform);

        if (GameManager.Instance.CurrentPlatform == MainPlatform.VR_WINDOWS)
        {
            VRPlayer.SetActive(true);

            Head.SetParent(VRHead);
            LeftHand.SetParent(VRLeftHand);
            RightHand.SetParent(VRRightHand);

            Head.localPosition = Vector3.zero;
            LeftHand.localPosition = Vector3.zero;
            RightHand.localPosition = Vector3.zero;

            Head.localRotation = Quaternion.identity;
            LeftHand.localRotation = Quaternion.identity;
            RightHand.localRotation = Quaternion.identity;
        }
        else if (GameManager.Instance.CurrentPlatform == MainPlatform.MOBILE)
        {
            ARPlayer.SetActive(true);
            Head.SetParent(ARHead);
            LeftHand.gameObject.SetActive(true);
            RightHand.gameObject.SetActive(true);
        }

        if (IsLocalPlayer)
        {
            Head.GetChild(0).gameObject.SetActive(false);
            LeftHand.GetChild(0).gameObject.SetActive(false);
            RightHand.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void ToggleMeshes(MeshRenderer[] renderers, bool activate)
    {
        foreach (var item in renderers)
        {
            item.enabled = activate;
        }
    }


}