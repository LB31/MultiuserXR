using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTailTest : MonoBehaviour
{
    public TrailRenderer Traily;

    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log(Traily.positionCount);
    }
}
