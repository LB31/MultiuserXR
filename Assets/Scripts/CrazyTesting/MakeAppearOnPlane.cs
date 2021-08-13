using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Moves the ARSessionOrigin in such a way that it makes the given content appear to be
    /// at a given location acquired via a raycast.
    /// </summary>
    [RequireComponent(typeof(ARSessionOrigin))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class MakeAppearOnPlane : MonoBehaviour
    {
        public GameObject ContentRepresentation;
        public Transform Content;

        [HideInInspector]
        public bool Recenter { get; set; } = true;
        public GameObject RecenterButton;
        public GameObject PlaceButton;

        private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        private ARSessionOrigin m_SessionOrigin;
        private ARRaycastManager m_RaycastManager;

        [SerializeField]
        [Tooltip("The rotation the content should appear to have.")]
        Quaternion m_Rotation;

        private bool placementPoseIsValid;
        private Pose placementPose;
        private bool isClient;


        /// <summary>
        /// The rotation the content should appear to have.
        /// </summary>
        public Quaternion rotation
        {
            get { return m_Rotation; }
            set
            {
                m_Rotation = value;
                if (m_SessionOrigin != null)
                    m_SessionOrigin.MakeContentAppearAt(Content, Content.transform.position, m_Rotation);
            }
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

            Invoke(nameof(CustomStart), 2);
        }

        private void CustomStart()
        {
            if (!isClient)
            {
                ContentRepresentation.SetActive(false);
                Content.gameObject.SetActive(true);
                return;
            }

            ContentRepresentation.SetActive(true);
            Content.gameObject.SetActive(false);
            
            RecenterButton.SetActive(false);
            PlaceButton.SetActive(true);
        }

        void Update()
        {
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

        public void PlaceObject()
        {
            if (!placementPoseIsValid) return;
            // This does not move the content; instead, it moves and orients the ARSessionOrigin
            // such that the content appears to be at the raycast hit position.
            m_SessionOrigin.MakeContentAppearAt(Content, placementPose.position, placementPose.rotation);

            RecenterButton.SetActive(true);
            PlaceButton.SetActive(false);
            Content.gameObject.SetActive(true);
            ContentRepresentation.SetActive(false);
            Recenter = false;
        }

        public void RestartPlacing()
        {
            RecenterButton.SetActive(false);
            PlaceButton.SetActive(true);
            Content.gameObject.SetActive(false);
            ContentRepresentation.SetActive(true);
            Recenter = true;
        }

        private void UpdatePlacementIndicator()
        {
            if (placementPoseIsValid)
            {
                ContentRepresentation.gameObject.SetActive(true);
                ContentRepresentation.gameObject.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                ContentRepresentation.gameObject.SetActive(false);
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
                placementPose = hits[0].pose;
            }


            if (placementPoseIsValid)
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
        }


    }
}