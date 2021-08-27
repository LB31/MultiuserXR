using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractableObject : NetworkBehaviour
{
    public bool PivotIsInMiddle;
    public GameObject SelectionReticle;
    [SerializeField]
    [Tooltip("Use absolute values. Like 10 -> 10 times bigger than original")]
    private Vector2 MinMaxScale;
    [HideInInspector]
    public float allowedMin;
    [HideInInspector]
    public float allowedMax;

    public Button ColorButton;

    public float MinDegrees = 1.5f;
    public float SnapDistance = 5f;

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

    public NetworkVariable<Vector3> ObjectRotation = new NetworkVariable<Vector3>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Vector3.zero);

    public NetworkVariable<Vector3> ObjectPosition = new NetworkVariable<Vector3>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Vector3.zero);

    [HideInInspector]
    public bool ClientRotates;
    [HideInInspector]
    public bool ClientScales;
    [HideInInspector]
    public bool ClientMoves;

    private Renderer Renderer;

    private float lerpTime;
    private Vector3 newRotationValue = Vector3.zero;
    private Vector3 newScaleValue = Vector3.one;
    private Vector3 newPositionValue = Vector3.one;

    private Vector3 originalScale;

    public void Awake()
    {

        // Prepare selection Reticle     
        Vector3 reticlePos = Vector3.zero;
        if (PivotIsInMiddle) reticlePos.y -= GetHeight() * 0.5f;
        SelectionReticle = Instantiate(SelectionReticle, transform);
        SelectionReticle.transform.localPosition = reticlePos;
        SelectionReticle.SetActive(false);

        Renderer = GetComponent<Renderer>();

        allowedMin = transform.localScale.y / MinMaxScale.x;
        allowedMax = transform.localScale.y * MinMaxScale.y;

        // Define start values
        if (IsServer || IsHost)
        {
            ObjectScale.Value = transform.localScale;
            ObjectRotation.Value = transform.rotation.eulerAngles;
            ObjectPosition.Value = transform.position;

            MaterialColor.Value = Renderer.material.color;
        }
        // Assign start values
        if (IsHost || IsServer || IsClient)
        {
            transform.localScale = ObjectScale.Value;
            transform.rotation = Quaternion.Euler(ObjectRotation.Value);
            transform.position = ObjectPosition.Value;

            Renderer.material.color = MaterialColor.Value;
        }


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
    }

    private void OnDisable()
    {
        MaterialColor.OnValueChanged -= ChangeLocalColor;
        ObjectScale.OnValueChanged -= ChangeLocalScale;
        ObjectRotation.OnValueChanged -= ChangeLocalRotation;
        ObjectPosition.OnValueChanged -= ChangeLocalPosition;
    }

    public float GetHeight()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        return mf.sharedMesh.bounds.size.y * transform.localScale.y;
    }

    private void ChangeLocalScale(Vector3 previousValue, Vector3 newValue)
    {
        // Avoid getting back own sent change
        if (ClientScales)
            return;

        newScaleValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalRotation(Vector3 previousValue, Vector3 newValue)
    {
        // Avoid getting back own sent change
        if (ClientRotates)
            return;

        newRotationValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalPosition(Vector3 previousValue, Vector3 newValue)
    {
        // Avoid getting back own sent change
        if (ClientMoves)
            return;

        newPositionValue = newValue;
        lerpTime = 0;
    }

    private void ChangeLocalColor(Color previousValue, Color newValue)
    {
        Renderer.material.color = newValue;
    }

    private void Update()
    {
        // When Client is scaling the object
        if (ClientScales)
            ObjectScale.Value = transform.localScale;
        // When Client is rotating the object
        else if (ClientRotates)
            ObjectRotation.Value = transform.rotation.eulerAngles;
        // When Client is moving the object
        else if (ClientMoves)
            ObjectPosition.Value = transform.position;

        
        // When Client is not scaling and the network value has changed
        if (!ClientScales && transform.localScale != ObjectScale.Value)
        {
            lerpTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, newScaleValue, lerpTime);
            // When scale has reached almost the new scale
            if (transform.localScale.x >= newScaleValue.x * 0.9f) lerpTime = 1;
        }

        // When Client is not rotating and the network value has changed
        else if (!ClientRotates && transform.rotation.eulerAngles != ObjectRotation.Value)
        {
            // Interpolate rotation
            lerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotationValue), lerpTime);
            // Finish lerping
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(newRotationValue)) < MinDegrees) lerpTime = 1;
        }

        // When Client is not moving and the network value has changed
        else if (!ClientMoves && transform.position != ObjectPosition.Value)
        {
            lerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPositionValue, lerpTime);
            if (Vector3.Distance(transform.position, newPositionValue) > SnapDistance) lerpTime = 1;
        }
        



    }

}
