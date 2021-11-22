using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRManager : Singleton<VRManager>
{
    [HideInInspector] public bool OculusInUse;
    [HideInInspector] public DeviceBasedSnapTurnProvider SnapTurnProvider;
    [HideInInspector] public XRInput XRInputLeft;
    [HideInInspector] public XRInput XRInputRight;
    public InputDevice LeftCon;
    public InputDevice RightCon;
    public InputFeatureUsage<Vector2> Axis2D;

    protected override void Awake()
    {
        base.Awake();       
    }

    private void OnEnable()
    {
        GetXRInputs();
    }

    private void GetXRInputs()
    {
        XRInput[] inputs = GetComponents<XRInput>();
        if (inputs.Length == 0) return;
        int indexLeft = Array.IndexOf(inputs, inputs.First(con => con.Controller.name.ToLower().Contains("left")));
        XRInputLeft = inputs[indexLeft];
        XRInputRight = inputs[indexLeft == 0 ? 1 : 0];
    }

    public void ChangeHeadsetDependencies()
    {
        SnapTurnProvider = GetComponent<DeviceBasedSnapTurnProvider>();
        if (OculusInUse)
        {
            SnapTurnProvider.turnUsage = DeviceBasedSnapTurnProvider.InputAxes.Primary2DAxis;
            Axis2D = CommonUsages.primary2DAxis;
        }

        else
        {
            SnapTurnProvider.turnUsage = DeviceBasedSnapTurnProvider.InputAxes.Secondary2DAxis;
            Axis2D = CommonUsages.secondary2DAxis;
        }         
    }

    // During the runtime sometimes new GameObjects are created for assigning positions
    // This mehtod destroys them
    public async void DestroyEmptyObjects(GameObject objToDestroy)
    {
        await Task.Delay(1000);
        Destroy(objToDestroy);
    }

    public async void DeactivateWithDelay(GameObject obj)
    {
        await Task.Delay(100);
        obj.SetActive(false);
    }
}

