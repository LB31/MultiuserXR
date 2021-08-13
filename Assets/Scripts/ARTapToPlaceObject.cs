using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject ObjectToMove;

    private ARRaycastManager arRaycastManager;

    private Pose placementPose;
    private bool placementPoseIsValid = false;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        ObjectToMove.SetActive(false);
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementPoseIsValid && Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        enabled = false;
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            ObjectToMove.SetActive(true);
            ObjectToMove.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            ObjectToMove.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        //arOrigin.Raycast(screenCenter, hits, TrackableType.Planes);
        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
            placementPoseIsValid = hits.Count > 0;

        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}
