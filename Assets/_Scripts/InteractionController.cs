using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public float RotationSpeed = 10;

    public void RotateObject(Transform hitObject, float rotation)
    {
        hitObject.Rotate(Vector3.up, rotation * Time.deltaTime * RotationSpeed);
    }

    public void ScaleObject(Transform hitObject, float zoom)
    {
        hitObject.localScale *= zoom;
    }

}
