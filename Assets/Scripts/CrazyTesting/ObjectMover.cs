using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ObjectMover : MonoBehaviour
{
    public Vector3 ContentToAppearAtPos;
    private GameObject ownObject;
    public GameObject OwnSceneObject {
        get { return ownObject; } 
        set {
            ownObject = value;
            PrepareARPlayer();
        }
    }
    public float MetersToMove = 1;
    public ARSessionOrigin SessionOrigin;
    public Transform ContentToRecenter;

    public Button Left;
    public Button Right;
    public Button Recenter;

    private void Start()
    {
        Left.onClick.AddListener(MoveLeft);
        Right.onClick.AddListener(MoveRight);
        Recenter.onClick.AddListener(RecenterOrigin);
    }

    private void PrepareARPlayer()
    {
        OwnSceneObject.transform.parent = Camera.main.transform;
        OwnSceneObject.transform.localPosition = Vector3.zero;
    }

    private void RecenterOrigin()
    {
        if (SessionOrigin != null)
        {
            Transform cam = Camera.main.transform;
            SessionOrigin.MakeContentAppearAt(ContentToRecenter, ContentToAppearAtPos, Quaternion.identity);
            Debug.Log(cam.forward);
        }
            
    }

    private void MoveLeft()
    {
        if (!OwnSceneObject) return;
        OwnSceneObject.transform.position -= new Vector3(MetersToMove, 0, 0);
    }

    private void MoveRight()
    {
        if (!OwnSceneObject) return;
        OwnSceneObject.transform.position += new Vector3(MetersToMove, 0, 0);
    }
}
