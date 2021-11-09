using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapObject : MonoBehaviour
{
    public List<StreetObject> StreetObjects = new List<StreetObject>();
}

[Serializable]
public class StreetObject
{
    public PlateType Type;
    public GameObject Object;
}