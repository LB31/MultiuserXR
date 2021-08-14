using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManagerVR : Singleton<GameManagerVR>
{
    public Transform Player;


    [HideInInspector] public bool OculusInUse;
    [HideInInspector] public DeviceBasedSnapTurnProvider SnapTurnProvider;
    [HideInInspector] public XRInput XRInputLeft;
    [HideInInspector] public XRInput XRInputRight;
    public InputDevice LeftCon;
    public InputDevice RightCon;
    public InputFeatureUsage<Vector2> Axis2D;

    public bool MoveSun { get; set; }

    protected override void Awake()
    {
        base.Awake();

        Player = FindObjectOfType<XRRig>().transform;

        GetXRInputs();
    }

    private void GetXRInputs()
    {
        XRInput[] inputs = FindObjectsOfType<XRInput>();
        int indexLeft = Array.IndexOf(inputs, inputs.First(con => con.controller.name.ToLower().Contains("left")));
        XRInputLeft = inputs[indexLeft];
        XRInputRight = inputs[indexLeft == 0 ? 1 : 0];
    }

    public void ChangeHeadsetDependencies()
    {
        SnapTurnProvider = FindObjectOfType<DeviceBasedSnapTurnProvider>();
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
    public async void DestryEmptyObjects(GameObject objToDestroy)
    {
        await Task.Delay(1000);
        Destroy(objToDestroy);
    }
}

