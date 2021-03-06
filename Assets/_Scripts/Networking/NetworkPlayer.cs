using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform Head;
    public Renderer HeadRenderer;
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

    [Header("Test Components")]
    public GameObject DesktopTester;
    public Transform DesktopHead;

    public NetworkVariable<Color> PlayerMaterialColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Color.white);

    public override void NetworkStart()
    {
        if (IsOwner)
        {
            PlayerMaterialColor.Value = new Color(Random.value, Random.value, Random.value);
            HeadRenderer.gameObject.SetActive(false);

        }
    }

    private async void Start()
    {
        await Task.Delay(1000);
        HeadRenderer.material.color = PlayerMaterialColor.Value;
    }

    public void PreparePlatformSpecificPlayer()
    {
        if (IsServer && !IsHost) return;

        Debug.Log("PreparePlatformSpecificPlayer " + GameManager.Instance.CurrentPlatform);

        var platform = GameManager.Instance.CurrentPlatform;

        if (platform == MainPlatform.VR_WINDOWS || platform == MainPlatform.VR_ANDROID)
        {
            VRPlayer.SetActive(true);

            SetHeadDependencies(VRHead);
            LeftHand.SetParent(VRLeftHand);
            RightHand.SetParent(VRRightHand);

            ZeroTransform();          
        }
        else if (platform == MainPlatform.MOBILE)
        {
            ARPlayer.SetActive(true);

            SetHeadDependencies(ARHead);
            LeftHand.gameObject.SetActive(false);
            RightHand.gameObject.SetActive(false);

            ZeroTransform();            
        }
        else if (platform == MainPlatform.DESKTOP)
        {
            DesktopTester.SetActive(true);

            SetHeadDependencies(DesktopHead);
            LeftHand.gameObject.SetActive(false);
            RightHand.gameObject.SetActive(false);

            ZeroTransform();       
        }

        // Hide representation objects for local player
        if (IsLocalPlayer)
        {
            //Head.GetChild(0).gameObject.SetActive(false);
            LeftHand.GetChild(0).gameObject.SetActive(false);
            RightHand.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void ZeroTransform()
    {
        Head.localPosition = Vector3.zero;
        LeftHand.localPosition = Vector3.zero;
        RightHand.localPosition = Vector3.zero;

        Head.localRotation = Quaternion.identity;
        LeftHand.localRotation = Quaternion.identity;
        RightHand.localRotation = Quaternion.identity;
    }

    private void SetHeadDependencies(Transform camera)
    {
        Head.SetParent(camera);
        GameManager.Instance.DrawCanvas.worldCamera = camera.GetComponent<Camera>();
    }

    public void ToggleMeshes(MeshRenderer[] renderers, bool activate)
    {
        foreach (var item in renderers)
        {
            item.enabled = activate;
        }
    }


}