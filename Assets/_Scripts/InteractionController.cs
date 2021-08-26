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

    public void ScaleObject(InteractableObject hitObject, float zoom)
    {
        // Check if objects is in desired scale range
        Vector3 expectedScale = hitObject.transform.localScale * zoom;
        if (expectedScale.y > hitObject.allowedMin && expectedScale.y < hitObject.allowedMax)
        {
            hitObject.transform.localScale *= zoom;
            if (hitObject.PivotIsInMiddle)
            {
                Vector3 curPos = hitObject.transform.localPosition;
                hitObject.transform.localPosition = new Vector3(curPos.x, curPos.y - hitObject.GetHeight() * 0.5f, curPos.z);
            }
                
        }
        
    }

}
