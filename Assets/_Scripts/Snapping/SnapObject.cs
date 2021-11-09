using MLAPI;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapObject : NetworkBehaviour
{
    public List<StreetObject> StreetObjects = new List<StreetObject>();

    public NetworkVariable<int> PlateType = new NetworkVariable<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, 0);

    public override void NetworkStart()
    {
        AssignPlate();
    }

    public void AssignPlate()
    {
        StreetObject plateType = StreetObjects.FirstOrDefault(t => t.Type.Equals((PlateType)PlateType.Value));
        plateType.Object.SetActive(true);
    }

}

[Serializable]
public class StreetObject
{
    public PlateType Type;
    public GameObject Object;
}