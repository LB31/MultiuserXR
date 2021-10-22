using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInteractionController : MonoBehaviour, IInteractionController
{
    private float rotationSpeed = 10;
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
        if (selectArgs.interactable.gameObject.HasComponent<InteractableObject>())
        {
            previousSelectedObject = selectedObject;
            selectedObject = selectArgs.interactable.gameObject.GetComponent<InteractableObject>();   

            // Is this object selected already by someone else?
            if (selectedObject.SelectedBy.Value != ulong.MaxValue &&
                selectedObject.SelectedBy.Value != NetworkManager.Singleton.LocalClientId)
            {
                selectedObject = null;
                return;
            }
            // Show that the object is now selected by the client
            else
            {
                Color ownColor = transform.root.GetComponent<NetworkPlayer>().MaterialColor.Value;
                selectedObject.SelectedBy.Value = NetworkManager.Singleton.LocalClientId;
                selectedObject.SelectionReticle.SetActive(true);
                selectedObject.SelectionReticle.GetComponent<SpriteRenderer>().color = ownColor;            
            }

            // When Client selects a new object
            if (previousSelectedObject && previousSelectedObject != selectedObject)
            {
                previousSelectedObject.SelectionReticle.SetActive(false);
                previousSelectedObject.SelectedBy.Value = ulong.MaxValue;
            }

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

            selectedObject.SelectedBy.Value = ulong.MaxValue;
            selectedObject.SelectionReticle.SetActive(false);
            selectedObject = null;
        }
    }

}
