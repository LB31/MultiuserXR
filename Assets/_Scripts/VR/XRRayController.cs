using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRayController : MonoBehaviour
{
    private XRController xrController;
    private XRRayInteractor rayInteractor;

    private LayerMask startLayers;
    private bool layerWasSet;

    private void Awake()
    {
        xrController = GetComponent<XRController>();
        rayInteractor = GetComponent<XRRayInteractor>();

        Initialize();
    }

    private void Initialize()
    {
        //xrController.moveObjectOut = InputHelpers.Button.SecondaryAxis2DUp;
        xrController.moveObjectOut = InputHelpers.Button.None;
        xrController.moveObjectIn = InputHelpers.Button.None;
        xrController.rotateObjectLeft = InputHelpers.Button.None;
        xrController.rotateObjectRight = InputHelpers.Button.None;
        //xrController.rotateObjectRight = InputHelpers.Button.None;

        startLayers = rayInteractor.raycastMask;
        rayInteractor.translateSpeed = 3;
        rayInteractor.raycastTriggerInteraction = QueryTriggerInteraction.Collide;
        //rayInteractor.maxRaycastDistance = 10;
        //rayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
    }

    private void Update()
    {
        // Allow only selction on interactable element
        if (xrController.selectInteractionState.active && !layerWasSet)
        {
            rayInteractor.raycastMask = LayerMask.GetMask("Nothing");
            layerWasSet = true;
        }          
        else if (!xrController.selectInteractionState.active && layerWasSet)
        {
            rayInteractor.raycastMask = startLayers;
            layerWasSet = false;
        }
            
    }
}
