using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractableObject : NetworkBehaviour
{
    public Button ColorButton;
    public Button BiggerButton;
    public Button SmallerButton;

    public NetworkVariable<Color> MaterialColor = new NetworkVariable<Color>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Color.white);

    public NetworkVariable<Vector3> OwnScale = new NetworkVariable<Vector3>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, Vector3.one);

    private Renderer Renderer;

    public void Awake()
    {
        Renderer = GetComponent<Renderer>();

        if (IsServer || IsHost)
            OwnScale.Value = transform.localScale;

        // Assign start values
        if (MaterialColor.Value != Color.white)
            Renderer.material.color = MaterialColor.Value;
        if (OwnScale.Value != Vector3.one)
        {
            transform.localScale = OwnScale.Value;
            Debug.Log(OwnScale.Value);
        }
            

        // Assign listeners for changed values
        MaterialColor.OnValueChanged += ChangeLocalColor;
        OwnScale.OnValueChanged += ChangeLocalScale;

        // Prepare interaction buttons
        // TODO maybe assign in inspector because of runtime condition with plane dragging
        ColorButton.onClick.AddListener(ChangeColor);
        BiggerButton.onClick.AddListener(() => ChangeScale(true));
        SmallerButton.onClick.AddListener(() => ChangeScale(false));
        Debug.Log("Prepare interaction buttons");
    }

    private void OnEnable()
    {
        MaterialColor.OnValueChanged += ChangeLocalColor;
        OwnScale.OnValueChanged += ChangeLocalScale;
    }

    private void OnDisable()
    {
        MaterialColor.OnValueChanged -= ChangeLocalColor;
        OwnScale.OnValueChanged -= ChangeLocalScale;
    }



    private void ChangeScale(bool bigger)
    {
        OwnScale.Value = bigger ? transform.localScale * 1.2f : transform.localScale / 1.2f;
    }
    private void ChangeColor()
    {
        MaterialColor.Value = Random.ColorHSV();
        Debug.Log("ChangeColor");
    }

    private void ChangeLocalScale(Vector3 previousValue, Vector3 newValue)
    {
        transform.localScale = newValue;
    }

    private void ChangeLocalColor(Color previousValue, Color newValue)
    {
        Renderer.material.color = newValue;
    }

}
