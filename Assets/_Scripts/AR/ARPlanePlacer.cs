using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


/// <summary>
/// Moves the ARSessionOrigin in such a way that it makes the given content appear to be
/// at a given location acquired via a raycast.
/// </summary>
[RequireComponent(typeof(ARSessionOrigin))]
[RequireComponent(typeof(ARRaycastManager))]
public class ARPlanePlacer : MonoBehaviour
{
    // Debug
    public bool Debugging;

    public GameObject DraggingPlane;
    public GameObject ContentRepresentation;
    private Transform content;
    private ARAnchorManager anchorManager;

    [HideInInspector]
    public bool Recenter { get; set; } = true;

    public GameObject RecenterButton;
    public GameObject PlaceButton;
    public GameObject PlaceTutorial;

    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private ARSessionOrigin m_SessionOrigin;
    private ARRaycastManager m_RaycastManager;



    private bool placementPoseIsValid;
    private Pose placementPose;
    private bool isClient;


    [SerializeField]
    [Tooltip("The rotation the content should appear to have.")]
    private Quaternion m_Rotation;

    public Quaternion Rotation
    {
        get { return m_Rotation; }
        set
        {
            m_Rotation = value;
            if (m_SessionOrigin != null)
                m_SessionOrigin.MakeContentAppearAt(content, content.transform.position, m_Rotation);
        }
    }

    private void OnEnable()
    {
        content = GameManager.Instance.WorldCenter;
        content.gameObject.SetActive(false);
        DraggingPlane.SetActive(false);

        ContentRepresentation = Instantiate(ContentRepresentation);
        ContentRepresentation.transform.localScale *= 2;
        ContentRepresentation.AddComponent<BoxCollider>();
        ContentRepresentation.AddComponent<OnTap>().Tapped += PlaceObject;

    }

    private void Start()
    {
        // For debugging
        if (NetworkManager.Singleton == null)
            isClient = true;
        else
            isClient = NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost;

        m_SessionOrigin = GetComponent<ARSessionOrigin>();
        m_RaycastManager = GetComponent<ARRaycastManager>();

        //Invoke(nameof(CustomStart), 0);
        CustomStart();
    }

    private void CustomStart()
    {
        // Show real content
        if (!isClient)
        {
            ContentRepresentation.SetActive(false);
            content.gameObject.SetActive(true);
            return;
        }

        // Show content representation for placement
        ContentRepresentation.SetActive(true);
        content.gameObject.SetActive(false);

        PlaceButton.SetActive(false);
        RecenterButton.SetActive(false);
        //PlaceButton.SetActive(true);
    }

    void Update()
    {
        // Return if server or when placement was paused
        if (!isClient || !Recenter) return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    // https://github.com/Unity-Technologies/arfoundation-samples/issues/25
    private bool IsPointOverAnyObject(Vector2 pos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0 || Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << 3);

    }

    // Currently called by PlaceButton Button Object
    public void PlaceObject()
    {
        if (!placementPoseIsValid) return;

        float angle = Quaternion.Angle(content.rotation, m_SessionOrigin.transform.rotation);
        placementPose.rotation.y = angle;
        m_SessionOrigin.MakeContentAppearAt(content, placementPose.position, placementPose.rotation);

        AddAnchor();

        TooglePlacing(false);
    }

    // Currently called by RecenterWorldButton Button Object
    public void TooglePlacing(bool startPlacing)
    {
        RecenterButton.SetActive(!startPlacing);
        PlaceButton.SetActive(startPlacing);
        content.gameObject.SetActive(!startPlacing);
        DraggingPlane.SetActive(!startPlacing);
        ContentRepresentation.SetActive(startPlacing); // Test !
        Recenter = startPlacing;
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            ContentRepresentation.SetActive(true);          
            PlaceButton.SetActive(true);
            PlaceTutorial.SetActive(false);

            //ContentRepresentation.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            ContentRepresentation.transform.position = placementPose.position;
        }
        else
        {
            ContentRepresentation.gameObject.SetActive(false);
            PlaceButton.SetActive(false);
            PlaceTutorial.SetActive(true);
        }
    }

    private void UpdatePlacementPose()
    {
        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        //arOrigin.Raycast(screenCenter, hits, TrackableType.Planes);
        if (m_RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            placementPoseIsValid = hits.Count > 0;
        }

        if (placementPoseIsValid)
        {
            // let object look in same z direction as camera
            //Vector3 newRotation = ContentRepresentation.transform.eulerAngles;
            //newRotation.y = Camera.main.transform.eulerAngles.y;
            //ContentRepresentation.transform.eulerAngles = newRotation;
            //return;
            placementPose = hits[0].pose;

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void AddAnchor()
    {
        if (!anchorManager) return;

        if (content.GetComponent<ARAnchor>() != null)
            Destroy(content.GetComponent<ARAnchor>());

        content.gameObject.AddComponent<ARAnchor>();
    }

}
