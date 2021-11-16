using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInteractionController : InteractionController
{

    public void SelectObject(SelectEnterEventArgs selectArgs)
    {
        bool selected = SelectObject(selectArgs.interactable.gameObject);

        selectedObject.ClientMoves = selected;
        selectedObject.ClientRotates = selected;
    }

    public void DeselectObject(SelectExitEventArgs selectArgs)
    {
        DeselectObject(true);
    }

}
