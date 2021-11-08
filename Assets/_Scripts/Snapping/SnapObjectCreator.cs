using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapObjectCreator : MonoBehaviour
{
    public GameObject PlateObject;
    public List<StreetObject> StreetObjects = new List<StreetObject>();
    private Transform sendingPlate;

    public void RegisterSender(Transform sender) => sendingPlate = sender;

    public void PlaceObject(int type)
    {
        StreetObject plateType = StreetObjects.FirstOrDefault(t => t.Type.Equals((PlateType)type));
        plateType.Object.SetActive(true);

        GameObject plate = Instantiate(PlateObject, transform);
        plate.transform.position = sendingPlate.position + sendingPlate.forward * 0.2f;
        plate.SetActive(true);

        SpawnObjectServerRPC(plate);
        CleanModel();
    }

    private void CleanModel()
    {
        foreach (var item in StreetObjects)
        {
            item.Object.SetActive(false);
        }
    }

    [ServerRpc]
    private void SpawnObjectServerRPC(GameObject obj)
    {
        obj.GetComponent<NetworkObject>().Spawn();
    }
}

[Serializable]
public class StreetObject
{
    public PlateType Type;
    public GameObject Object;
}