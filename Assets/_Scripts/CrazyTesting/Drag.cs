using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{

    public Vector2 zValues;
    public float dragSpeed = 1f;
    Vector3 lastMousePos;

    void OnMouseDown()
    {
        lastMousePos = Input.mousePosition;
    }

    // https://answers.unity.com/questions/1760290/drag-object-along-z-axis-using-mouse.html
    void OnMouseDrag()
    {
        Vector3 delta = Input.mousePosition - lastMousePos;
        Vector3 pos = transform.position;
        pos.z += delta.y * dragSpeed;
        pos.x += delta.x * dragSpeed;
        transform.position = pos;
        lastMousePos = Input.mousePosition;
    }

}
