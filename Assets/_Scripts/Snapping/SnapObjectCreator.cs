using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapObjectCreator : NetworkBehaviour
{
    public NetworkObject PlateObject;

    public GameObject CityButton;
    public List<Transform> ButtonRows;

    public GameObject SnapZone;
    public Transform SnapZoneParent;
    [Tooltip("X = width; Y = height")]
    public Vector2 SnapFieldSize = new Vector3(5, 5);
    public float SnapZoneSize = 0.2f;

    
    


    private Transform sendingPlate;

    private void Start()
    {
        CreateSnapField();
    }

    private void CreateSnapField()
    {
        for (int i = 0; i < SnapFieldSize.x; i++)
        {
            for (int j = 0; j < SnapFieldSize.y; j++)
            {
                GameObject snapZone = Instantiate(SnapZone, SnapZoneParent);
                snapZone.transform.localPosition = new Vector3(i * SnapZoneSize, 0, j * SnapZoneSize);
            }
        }
    }

    private void CreateSnapButtons()
    {

    }

    // Called by button
    public void RegisterSender(Transform sender) => sendingPlate = sender;
    // Called by button
    public void PlaceObject(int type)
    {
        SpawnObjectServerRpc(type, sendingPlate.position, sendingPlate.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectServerRpc(int type, Vector3 pos, Vector3 forward)
    {
        Vector3 newPos = pos + forward * 0.1f;
        NetworkObject plate = Instantiate(PlateObject, newPos, Quaternion.identity, transform);

        SnapObject snapObj = plate.GetComponent<SnapObject>();
        snapObj.PlateType.Value = type;
        snapObj.AssignPlate();

        //plate.transform.position = pos + forward * 0.1f;

        plate.Spawn();
    }
}

