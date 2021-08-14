using System;
using UnityEngine;

public class XRControls : Singleton<XRControls>
{
    public GameObject VRInteractionRays;

    public delegate void RightControllerButton(bool side);
    public delegate void ControllerTrigger();
    public delegate void ControllerGrip();
    public delegate void ControllerMenu();


    public event RightControllerButton ControllerEventButton; // Here A and B
    public event ControllerTrigger ControllerEventTrigger;
    public event ControllerGrip ControllerEventGrip;
    public event ControllerMenu ControllerEventMenu;

    private XRBinding[] bindingsTrigger = new XRBinding[2];
    private XRBinding[] bindingsButtons = new XRBinding[2];
    private XRBinding bindingMenu;

    public void RegisterInteraction()
    {
        if (bindingsTrigger[0] != null) return; // avoid 2 assigments

        GameManagerVR.Instance.XRInputRight.bindings.
            Add(bindingsTrigger[0] = new XRBinding(XRButton.Trigger, PressType.End, () => ControllerEventTrigger()));
        GameManagerVR.Instance.XRInputLeft.bindings.
            Add(bindingsTrigger[1] = new XRBinding(XRButton.Trigger, PressType.End, () => ControllerEventTrigger()));
    }

    private void RegisterGrabbing()
    {
        GameManagerVR.Instance.XRInputRight.bindings.
            Add(new XRBinding(XRButton.GripButton, PressType.Begin, () => ControllerEventGrip()));
    }

    private void RegisterSticks()
    {

    }

    public void RegisterMenuButton()
    {
        if (bindingMenu != null) return;

        GameManagerVR.Instance.XRInputLeft.bindings.
            Add(bindingMenu = new XRBinding(XRButton.Menu, PressType.End, () => ControllerEventMenu()));
    }

    public void RegisterButtonEvents()
    {
        if (bindingsButtons[0] != null) return;

        XRButton teleportLeft;
        XRButton teleportRight;

        if (GameManagerVR.Instance.OculusInUse)
        {
            teleportLeft = XRButton.SecondaryButton;
            teleportRight = XRButton.PrimaryButton;
        }
        // WMR Headset
        else
        {
            // actually not used here
            teleportLeft = XRButton.Primary2DAxisClick;

            teleportRight = XRButton.Primary2DAxisClick;
        }

        GameManagerVR.Instance.XRInputRight.bindings.
            Add(bindingsButtons[0] = new XRBinding(teleportRight, PressType.End, () => ControllerEventButton(true)));

        if (GameManagerVR.Instance.OculusInUse)
        {
            GameManagerVR.Instance.XRInputRight.bindings.
                Add(bindingsButtons[1] = new XRBinding(teleportLeft, PressType.End, () => ControllerEventButton(false)));
        }

    }

    public void RemoveButtonEvents()
    {
        if (ControllerEventButton == null) return;
        foreach (Delegate deli in ControllerEventButton.GetInvocationList())
        {
            ControllerEventButton -= (RightControllerButton)deli;
        }

    }

    public void RemoveTriggerEvents()
    {
        if (ControllerEventTrigger == null) return;
        foreach (Delegate deli in ControllerEventTrigger.GetInvocationList())
        {
            ControllerEventTrigger -= (ControllerTrigger)deli;
        }

    }



}
