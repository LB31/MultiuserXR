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

    public Button ColorButton;

    public float MinDegrees = 1.5f;

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


    public bool ClientRotates;
    public bool ClientScales;

    private Renderer Renderer;

    private float lerpTime;
    private Vector3 newRotationValue = Vector3.zero;
    private Vector3 newScaleValue = Vector3.one;


    public void Awake()
    {

        // Prepare selection Reticle
        MeshFilter mf = GetComponent<MeshFilter>();
        float objHeight = mf.sharedMesh.bounds.size.y * transform.localScale.y;
        Vector3 reticlePos = Vector3.zero;
        if (PivotIsInMiddle) reticlePos.y -= objHeight * 0.5f;
        SelectionReticle = Instantiate(SelectionReticle, transform);
        SelectionReticle.transform.localPosition = reticlePos;
        SelectionReticle.SetActive(false);

        Renderer = GetComponent<Renderer>();

        // Define start values
        if (IsServer || IsHost)
        {
            ObjectScale.Value = transform.localScale;
            MaterialColor.Value = Renderer.material.color;
            ObjectRotation.Value = transform.rotation.eulerAngles;
        }
        // Assign start values
        if (IsHost || IsServer || IsClient)
        {
            transform.localScale = ObjectScale.Value;
            transform.rotation = Quaternion.Euler(ObjectRotation.Value);
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
    }

    private void OnDisable()
    {
        MaterialColor.OnValueChanged -= ChangeLocalColor;
        ObjectScale.OnValueChanged -= ChangeLocalScale;
        ObjectRotation.OnValueChanged -= ChangeLocalRotation;
    }



    private void ChangeScale(bool bigger)
    {
        ObjectScale.Value = bigger ? transform.localScale * 1.2f : transform.localScale / 1.2f;
    }
    private void ChangeColor()
    {
        MaterialColor.Value = Random.ColorHSV();
        Debug.Log("ChangeColor");
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

    private void ChangeLocalColor(Color previousValue, Color newValue)
    {
        Renderer.material.color = newValue;
    }

    private void Update()
    {
        // When Client is scaling
        if (ClientScales)
            ObjectScale.Value = transform.localScale;
        // When Client is rotating
        else if (ClientRotates)
            ObjectRotation.Value = transform.rotation.eulerAngles;

        // When Client is not rotating and the network value has changed

        if (!ClientScales && transform.localScale != ObjectScale.Value)
        {
            lerpTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, newScaleValue, lerpTime);
            if (transform.localScale.x >= newScaleValue.x * 0.9f) lerpTime = 1;
        }
        if (!ClientRotates && transform.rotation.eulerAngles != ObjectRotation.Value)
        {
            // Interpolate rotation
            lerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotationValue), lerpTime);
            // Finish lerping
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(newRotationValue)) < MinDegrees) lerpTime = 1;
        }
        



    }

}
