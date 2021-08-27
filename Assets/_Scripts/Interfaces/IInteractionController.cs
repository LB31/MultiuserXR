using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractionController
{
    public float RotationSpeed { get; set; }

    public void RotateObject();
    public void ScaleObject();
    public void MoveObject();

}
