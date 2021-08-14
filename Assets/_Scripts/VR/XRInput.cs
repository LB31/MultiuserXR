using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInput : MonoBehaviour
{
#pragma warning disable 0649
    public XRController controller;
    public List<XRBinding> bindings;
#pragma warning restore 0649

    private void Update()
    {
        foreach (var binding in bindings)
            binding.Update(controller.inputDevice);
    }
}

[Serializable]
public class XRBinding
{
#pragma warning disable 0649
    [SerializeField] XRButton button;
    [SerializeField] PressType pressType;
    [SerializeField] UnityEvent OnActive = new UnityEvent();
#pragma warning restore 0649

    bool isPressed;
    bool wasPressed;

    public XRBinding(XRButton button, PressType pressType, UnityAction eventMethod)
    {
        this.button = button;
        this.pressType = pressType;
        OnActive.AddListener(eventMethod);
    }

    public void Update(InputDevice device)
    {
        device.TryGetFeatureValue(XRStatics.GetFeature(button), out isPressed);
        bool active = false;

        switch (pressType)
        {
            case PressType.Continuous: active = isPressed; break;
            case PressType.Begin: active = isPressed && !wasPressed; break;
            case PressType.End: active = !isPressed && wasPressed; break;
        }

        if (active) OnActive.Invoke();
        wasPressed = isPressed;
    }
}

public enum XRButton
{
    Trigger,
    GripButton,
    PrimaryButton,
    PrimaryTouch,
    SecondaryButton,
    SecondaryTouch,
    Primary2DAxisClick,
    Primary2DAxisTouch,
    Menu
}

public enum PressType
{
    Begin,
    End,
    Continuous
}

public static class XRStatics
{
    public static InputFeatureUsage<bool> GetFeature(XRButton button)
    {
        switch (button)
        {
            case XRButton.Trigger: return CommonUsages.triggerButton;
            case XRButton.GripButton: return CommonUsages.gripButton;
            case XRButton.PrimaryButton: return CommonUsages.primaryButton;
            case XRButton.PrimaryTouch: return CommonUsages.primaryTouch;
            case XRButton.SecondaryButton: return CommonUsages.secondaryButton;
            case XRButton.SecondaryTouch: return CommonUsages.secondaryTouch;
            case XRButton.Primary2DAxisClick: return CommonUsages.primary2DAxisClick;
            case XRButton.Primary2DAxisTouch: return CommonUsages.primary2DAxisTouch;
            case XRButton.Menu: return CommonUsages.menuButton;
            default: Debug.LogError("button " + button + " not found"); return CommonUsages.triggerButton;
        }
    }
}