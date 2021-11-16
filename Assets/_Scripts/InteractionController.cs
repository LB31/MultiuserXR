using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionController : MonoBehaviour
{
    protected InteractableObject selectedObject;
    protected InteractableObject previousSelectedObject;

    protected bool SelectObject(GameObject interactable)
    {
        // only hit objects that are shared through network
        if (interactable.HasComponent<InteractableObject>())
        {
            previousSelectedObject = selectedObject;
            selectedObject = interactable.GetComponent<InteractableObject>();
            selectedObject.SelectionReticle.SetActive(true);

            // Is this object selected already by someone else?
            if (selectedObject.SelectedBy.Value != ulong.MaxValue &&
                selectedObject.SelectedBy.Value != NetworkManager.Singleton.LocalClientId)
            {
                selectedObject = null;
                return false;
            }
            // Show that the object is now selected by the client
            else
            {
                Color ownColor = transform.root.GetComponent<NetworkPlayer>().PlayerMaterialColor.Value;
                selectedObject.SelectedBy.Value = NetworkManager.Singleton.LocalClientId;
                selectedObject.ReticleColor.Value = ownColor;
                selectedObject.SelectionReticle.GetComponent<SpriteRenderer>().color = ownColor;
            }

            // When Client selects a new object
            if (previousSelectedObject && previousSelectedObject != selectedObject)
            {
                previousSelectedObject.SelectionReticle.SetActive(false);
                previousSelectedObject.SelectedBy.Value = ulong.MaxValue;
            }

            return true;
        }

        return false;
    }


    protected async void DeselectObject(bool wholeDeselection)
    {
        await Task.Delay(300);
        if (selectedObject)
        {
            selectedObject.ClientRotates = false;
            selectedObject.ClientScales = false;
            selectedObject.ClientMoves = false;

            if (!wholeDeselection) return;

            selectedObject.SelectedBy.Value = ulong.MaxValue;
            selectedObject.SelectionReticle.SetActive(false);
            selectedObject = null;
        }
    }
}
