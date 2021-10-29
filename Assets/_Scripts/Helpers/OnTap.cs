using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTap : MonoBehaviour
{
    public event Action Tapped;

    private void OnMouseDown()
    {
        Tapped();
    }
}
