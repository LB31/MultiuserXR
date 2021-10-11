using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInteractionController : MonoBehaviour, IInteractionController
{
    public float rotationSpeed = 10;
    public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }

    private InteractableObject selectedObject;
    private InteractableObject previousSelectedObject;

    public void MoveObject()
    {
    }

    public void RotateObject()
    {
    }

    public void ScaleObject()
    {
    }

    public void SelectObject(SelectEnterEventArgs selectArgs)
    {
        // only hit objects that are shared through network
        if (selectArgs.interactable.gameObject.GetComponent<InteractableObject>())
        {
            previousSelectedObject = selectedObject;
            selectedObject = selectArgs.interactable.gameObject.GetComponent<InteractableObject>();
            selectedObject.SelectionReticle.SetActive(true);

            // Test
            selectedObject.ClientMoves = true;
            selectedObject.ClientRotates = true;
        }
    }

    public async void DeselectObject(SelectExitEventArgs selectArgs)
    {
        await Task.Delay(300);
        if (selectedObject)
        {
            selectedObject.ClientRotates = false;
            selectedObject.ClientScales = false;
            selectedObject.ClientMoves = false;
        }
    }

}
