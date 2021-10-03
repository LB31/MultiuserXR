using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PositionManager : Singleton<PositionManager>
{
    // Teleport Positions for Vero
    public List<CustomTransform> VeroPositions;
    public List<Vector3> BoatPositions = new List<Vector3>()
    {
        new Vector3(70f, 2.8f, 97f), // boat right origin
        new Vector3(87f, 2.8f, 93f), // boat right temple
        new Vector3(72f, 2.8f, 103f), // boat left origin
        new Vector3(87f, 2.8f, 100f), // boat left temple
    };

    public float FadeSpeedSceneChanger = 1f;

    private Image sceneChanger;

    protected override void Awake()
    {
        base.Awake();

        GameObject switchCanvas = GameObject.Find("WorldSwitchCanvas");
        sceneChanger = switchCanvas != null ? switchCanvas.transform.GetChild(0).GetComponent<Image>() : null;
    }

    public void TeleportCharacter(Transform chartToMove, Transform nextLocation)
    {
        chartToMove.GetComponent<CharacterController>().enabled = false;
        chartToMove.position = nextLocation.position;
        chartToMove.rotation = nextLocation.localRotation;
        chartToMove.GetComponent<CharacterController>().enabled = true;
    }
    public void TeleportCharacter(Transform chartToMove, Vector3 nextPos)
    {
        chartToMove.GetComponent<CharacterController>().enabled = false;
        chartToMove.position = nextPos;
        chartToMove.GetComponent<CharacterController>().enabled = true;
    }

    public void TeleportNavAgent(Transform chartToMove, Vector3 nextPos)
    {
        chartToMove.GetComponent<NavMeshAgent>().enabled = false;
        chartToMove.position = nextPos;
        chartToMove.GetComponent<NavMeshAgent>().enabled = true;
    }

    public void TeleportObject(GameObject objToMove, Vector3 nextPos)
    {
        objToMove.transform.position = nextPos;
    }

    public Transform GetNextTransform(Location location)
    {
        CustomTransform nextTransform = VeroPositions.Find(l => l.Location == location);
        Transform nextVeroPos = new GameObject().transform;
        nextVeroPos.position = nextTransform.Position;
        nextVeroPos.rotation = Quaternion.Euler(nextTransform.Rotation);

        GameManagerVR.Instance.DestroyEmptyObjects(nextVeroPos.gameObject);

        return nextVeroPos;
    }

    public async void ChangeVeroPosition(Action<Transform, Vector3> method, Transform charToMove, Vector3 nextPos)
    {
        await VisualizeSceneChange(true);
        method(charToMove, nextPos);
        await VisualizeSceneChange(false);
    }

    public async void ChangeVeroPosition(Action<Transform, Transform> method, Transform charToMove, Transform nextTransform)
    {
        await VisualizeSceneChange(true);
        method(charToMove, nextTransform);
        await VisualizeSceneChange(false);
    }

    public void ChangePositionFSM(GameObject objToMove, Vector3 nextPos)
    {
        ChangeVeroPosition(TeleportCharacter, objToMove.transform, nextPos);
    }


    public async Task VisualizeSceneChange(bool forth)
    {
        sceneChanger.enabled = true;
        Color color = sceneChanger.color;

        if (forth)
        {
            for (float i = 0; i <= 1; i += Time.deltaTime * FadeSpeedSceneChanger)
            {
                color.a = i;
                sceneChanger.color = color;
                await Task.Yield();
            }
            color.a = 1;
        }
        else
        {
            await Task.Delay(1000);
            for (float i = 1; i >= 0; i -= Time.deltaTime * FadeSpeedSceneChanger)
            {
                color.a = i;
                sceneChanger.color = color;
                await Task.Yield();
            }
            color.a = 0;
        }
        
        sceneChanger.color = color;

        sceneChanger.enabled = forth;
    }


}

[Serializable]
public class CustomTransform
{
    public Location Location;
    public Vector3 Position;
    public Vector3 Rotation;
}

public enum Location
{
    Temple,
    Harbor,
    PartyYurt, 
    GrannyYurt
}
