using MLAPI;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XROffsetGrabInteractable))]
public class InteractableObject : NetworkBehaviour
{
    public bool PivotIsInMiddle;
    public GameObject SelectionReticle;
    [Tooltip("X times as big or small")]
    public float SelectionReticleSize = 1;
    public bool ScalingAllowed = true;
    [SerializeField]
    [Tooltip("Use absolute values. Like 10 -> 10 times bigger than original")]
    private Vector2 MinMaxScale;
    [HideInInspector]
    public float AllowedMin;
    [HideInInspector]
    public float AllowedMax;

    [Header("Network Fields")]
    public NetworkVariable<Color> MaterialColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Color.white);
    public NetworkVariable<Vector3> ObjectScale = new NetworkVariable<Vector3>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Vector3.one);
    public NetworkVariable<Quaternion> ObjectRotation = new NetworkVariable<Quaternion>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Quaternion.identity);
    public NetworkVariable<Vector3> ObjectPosition = new NetworkVariable<Vector3>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Vector3.zero);
    // Network ID of player who has selected this object currently
    public NetworkVariable<ulong> SelectedBy = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, ulong.MaxValue);
    public NetworkVariable<Color> ReticleColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Color.red);

    [HideInInspector]
    public bool ClientRotates;
    [HideInInspector]
    public bool ClientScales;
    [HideInInspector]
    public bool ClientMoves;

    private Renderer Renderer;

    private float lerpTime;
    private Vector3 newPositionValue = Vector3.one;
    private Quaternion newRotationValue = Quaternion.identity;
    private Vector3 newScaleValue = Vector3.one;
    

    private Vector3 originalScale;

    public async void Awake()
    {
        // Prepare selection Reticle     
        Vector3 reticlePos = Vector3.zero;
        if (PivotIsInMiddle) reticlePos.y -= GetHeight() * 0.5f;
        SelectionReticle = Instantiate(SelectionReticle, transform);
        SelectionReticle.transform.localPosition = reticlePos;
        SelectionReticle.transform.localScale *= SelectionReticleSize != 0 ? SelectionReticleSize : 1;
        SelectionReticle.SetActive(false);

        Renderer = GetComponent<Renderer>();

        gameObject.layer = LayerMask.NameToLayer("Grab");

        AllowedMin = transform.localScale.y / MinMaxScale.x;
        AllowedMax = transform.localScale.y * MinMaxScale.y;

        //transform.parent = null;

        // Define start values
        if (IsServer || IsHost)
        {
            ObjectPosition.Value = transform.position;
            ObjectRotation.Value = transform.rotation;     
            ObjectScale.Value = transform.localScale;
            //MaterialColor.Value = Renderer.material.color;
        }

        await Task.Delay(1000);

        // Assign start values for new clients       
        transform.position = ObjectPosition.Value;
        transform.rotation = ObjectRotation.Value;
        transform.localScale = ObjectScale.Value;

        //Renderer.material.color = MaterialColor.Value;

        NetworkManager.Singleton.OnClientDisconnectCallback += RevemoSelectionOnDisconnect;

        // Prepare interaction buttons
        // TODO maybe assign in inspector because of runtime condition with plane dragging
        //ColorButton.onClick.AddListener(ChangeColor); 
    }



    private void OnEnable()
    {
        // Assign listeners for changed values
        MaterialColor.OnValueChanged += ChangeLocalColor;
        ObjectScale.OnValueChanged += ChangeLocalScale;
        ObjectRotation.OnValueChanged += ChangeLocalRotation;
        ObjectPosition.OnValueChanged += ChangeLocalPosition;
        SelectedBy.OnValueChanged += ChangeSelectionOwner;
    }

    private void OnDisable()
    {
        MaterialColor.OnValueChanged -= ChangeLocalColor;
        ObjectScale.OnValueChanged -= ChangeLocalScale;
        ObjectRotation.OnValueChanged -= ChangeLocalRotation;
        ObjectPosition.OnValueChanged -= ChangeLocalPosition;
        SelectedBy.OnValueChanged -= ChangeSelectionOwner;
    }

    public float GetHeight()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        return mf.sharedMesh.bounds.size.y * transform.localScale.y;
    }

    private void ChangeLocalPosition(Vector3 previousValue, Vector3 newValue)
    {
        // Avoid getting back own sent change
        if (ClientMoves)
            return;

        newPositionValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalRotation(Quaternion previousValue, Quaternion newValue)
    {
        // Avoid getting back own sent change
        if (ClientRotates)
            return;

        newRotationValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalScale(Vector3 previousValue, Vector3 newValue)
    {
        // Avoid getting back own sent change
        if (ClientScales)
            return;

        newScaleValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalColor(Color previousValue, Color newValue)
    {
        if (newValue != Color.white)
            Renderer.material.color = newValue;
    }

    private void ChangeSelectionOwner(ulong previousValue, ulong newValue)
    {
        // Was the object selected by local player?
        if (NetworkManager.Singleton.LocalClientId == newValue)
            return;
        // Object was deselcted
        else if (newValue == ulong.MaxValue)
        {
            SelectionReticle.SetActive(false);
        }
        // Another player selected this object
        else
        {
            SelectionReticle.GetComponent<SpriteRenderer>().color = ReticleColor.Value;
            SelectionReticle.SetActive(true);
        }
    }

    private void RevemoSelectionOnDisconnect(ulong leavingClient)
    {
        if (SelectedBy.Value.Equals(leavingClient))
        {
            SelectedBy.Value = ulong.MaxValue;
            SelectionReticle.SetActive(false);
        }
    }

    private void Update()
    {
        // When Client is moving the object
        if (ClientMoves)
            ObjectPosition.Value = transform.position;
        // When Client is rotating the object
        if (ClientRotates)
            ObjectRotation.Value = transform.rotation;
        // When Client is scaling the object
        if (ClientScales)
            ObjectScale.Value = transform.localScale;
        
        // When Client is not moving and the network value was changed
        if (!ClientMoves && transform.position != ObjectPosition.Value)
        {
            lerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPositionValue, lerpTime);
        }
        // When Client is not rotating and the network value was changed
        if (!ClientRotates && transform.rotation != ObjectRotation.Value)
        {
            // Interpolate rotation
            lerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotationValue, lerpTime);
        }
        // When Client is not scaling and the network value was changed
        if (!ClientScales && transform.localScale != ObjectScale.Value)
        {
            lerpTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, newScaleValue, lerpTime);
        }
    }

}
