using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SnapController : MonoBehaviour
{
    public GameObject SnapVisualization;

    private bool triggered;
    private bool placed;
    private Transform placedObject;


    private void Start()
    {
        SnapVisualization = Instantiate(SnapVisualization, transform);
        SnapVisualization.SetActive(false);
    }

    private async void OnTriggerStay(Collider other)
    {
        // Return when zone was not triggered or there is already an object placed
        if (!triggered || placed) return;
        // When interactable object was the trigger
        if (other.gameObject.HasComponent<InteractableObject>())
        {
            var io = other.GetComponent<InteractableObject>();
            // Return when object is still being moved
            if (IsObjectSelected(other)) return;

            // Show that object is now in zone
            placed = true;
            placedObject = other.transform;
            SnapVisualization.SetActive(false);
            triggered = false;

            // Simulate movement for network         
            io.ClientMoves = true;
            io.ClientRotates = true;

            // Snap object inside of zone
            Vector3 snapPos = transform.position;
            other.transform.position = new Vector3(snapPos.x, snapPos.y + 0.01f, snapPos.z);
            // Snap rotation to the nearest 90 degrees
            Vector3 curRot = other.transform.eulerAngles;
            curRot.x = curRot.z = 0;
            curRot.y = Mathf.Round(curRot.y / 90) * 90;
            other.transform.eulerAngles = curRot;

            // Wait for next frame before stop sending
            await Task.Delay(100);
            io.ClientMoves = false;
            io.ClientRotates = false;

            Deselect(io);
        }
    }

    private void Deselect(InteractableObject selectedObject)
    {
        selectedObject.SelectedBy.Value = ulong.MaxValue;
        selectedObject.SelectionReticle.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        // When not an InteractableObject has triggered
        if (!other.gameObject.HasComponent<InteractableObject>()) return;
        // Return when another than the placed object exits the zone
        if (placed && other.transform != placedObject) return;

        triggered = false;
        placed = false;
        SnapVisualization.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return when there is already an object in zone
        if (placed) return;
        // When not an InteractableObject has triggered
        if (!other.gameObject.HasComponent<InteractableObject>()) return;

        triggered = true;
        SnapVisualization.SetActive(true);
    }

    private bool IsObjectSelected(Collider other)
    {
        if (!other.gameObject.HasComponent<InteractableObject>()) return false;
        var io = other.GetComponent<InteractableObject>();
        return io.SelectedBy.Value != ulong.MaxValue;
    }

    private bool IsLocalClientMoving(Collider other)
    {
        if (!other.gameObject.HasComponent<InteractableObject>()) return false;

        var io = other.GetComponent<InteractableObject>();
        return io.SelectedBy.Value == NetworkManager.Singleton.LocalClientId;
    }
}
