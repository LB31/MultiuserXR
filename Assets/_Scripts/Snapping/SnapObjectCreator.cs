using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapObjectCreator : NetworkBehaviour
{
    public GameObject SnapZone;
    public Transform SnapZoneParent;
    [Tooltip("X = width; Y = height")]
    public Vector2 SnapFieldSize = new Vector3(5, 5);
    public float SnapZoneSize = 0.2f;

    public GameObject PlateObject;
    


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
        GameObject plate = Instantiate(PlateObject, transform);
        SnapObject snapObj = plate.GetComponent<SnapObject>();

        StreetObject plateType = snapObj.StreetObjects.FirstOrDefault(t => t.Type.Equals((PlateType)type));
        plateType.Object.SetActive(true);
   
        plate.transform.position = pos + forward * 0.1f;
        plate.SetActive(true);

        plate.GetComponent<NetworkObject>().Spawn();
    }
}

