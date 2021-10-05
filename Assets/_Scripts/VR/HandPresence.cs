using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPresence : MonoBehaviour
{
    public bool HideController;

    public InputDeviceCharacteristics ControllerCharacteristics;
    public List<GameObject> ControllerPrefabs;
    public GameObject HandModelPrefab;

    private InputDevice targetDevice;
    private GameObject spawnedController;
    private GameObject spawnedHand;
    private Animator handAnimator;

    public bool offline;
    private bool isOwner;

    private void Awake()
    {
        if (offline) isOwner = true;
        else
            isOwner = GetComponentInParent<VRPlayer>().IsOwner;


        TryInitialize();
    }

    void TryInitialize()
    {
        if (!isOwner)
        {
            HideController = true;
            spawnedHand = Instantiate(HandModelPrefab, transform);
            handAnimator = spawnedHand.GetComponent<Animator>();
            return;
        }

        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(ControllerCharacteristics, devices);

        if (devices.Count == 0) return;
        targetDevice = devices[0];

        if (targetDevice.name.ToLower().Contains("oculus"))
            GameManagerVR.Instance.OculusInUse = true;

        GameManagerVR.Instance.ChangeHeadsetDependencies();

        GameObject prefab = ControllerPrefabs.Find(controller => controller.name == targetDevice.name);

        if (prefab)
            spawnedController = Instantiate(prefab, transform);
        // Use the default controller
        else
            spawnedController = Instantiate(ControllerPrefabs[0], transform);

        if (HideController)
        {
            spawnedHand = Instantiate(HandModelPrefab, transform);
            handAnimator = spawnedHand.GetComponent<Animator>();
            spawnedController.SetActive(false);
        }

        if (targetDevice.name.ToLower().Contains("left"))
        {
            GameManagerVR.Instance.LeftCon = targetDevice;
            //GameStateHandler.Instance.RigsterMenu();
        }

        if (targetDevice.name.ToLower().Contains("right"))
        {
            GameManagerVR.Instance.RightCon = targetDevice;
        }

        return;
        // TODO do it smarter
        XRControls.Instance.RegisterButtonEvents();
        XRControls.Instance.RegisterInteraction();
        XRControls.Instance.RegisterMenuButton();

    }

    [ContextMenu("ON")]
    public void On() => ToggleHands(true);

    [ContextMenu("OFF")]
    public void Off() => ToggleHands(false);

    public void ToggleHands(bool toHands)
    {
        if (toHands)
        {
            // When there is already a hand
            if (spawnedHand)
            {
                spawnedHand.SetActive(true);
                spawnedController.SetActive(false);
                return;
            }
            spawnedHand = Instantiate(HandModelPrefab, transform);
            handAnimator = spawnedHand.GetComponent<Animator>();
            HideController = true;
            spawnedController.SetActive(false);
        }
        else
        {
            if (spawnedHand)
            {
                spawnedController.SetActive(true);
                spawnedHand.SetActive(false);
                HideController = false;
            }
        }
    }

    private void UpdateHandAnimation()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal))
            handAnimator.SetFloat("Trigger", triggerVal);
        else
            handAnimator.SetFloat("Trigger", 0);

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripVal))
            handAnimator.SetFloat("Grip", gripVal);
        else
            handAnimator.SetFloat("Grip", 0);
    }

    void Update()
    {
        if (!targetDevice.isValid && isOwner)
            TryInitialize();
        else if (HideController)
            UpdateHandAnimation();

        //targetDevice.TryGetFeatureValue(CommonUsages.menuButton, out bool primaryButtonVal);
        //if (primaryButtonVal)
        //    print("Pressing menu");

        //targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal);
        //if (triggerVal > 0.5f)
        //    print("trigger " + triggerVal);

        //targetDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 trigger2DVal);
        //if (trigger2DVal != Vector2.zero)
        //    print("2d axis" + trigger2DVal);
    }

}
