using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PositionChanger : MonoBehaviour
{
    public List<AreaPositions> AreaPositions;

    // Debug
    public Transform CurrentVeroPosition;

    private List<Transform> currentPositions;
    private int currentPosition;

    //private XRBinding teleportBindingLeft;
    private XRBinding teleportBindingRightA;
    private XRBinding teleportBindingRightB;

    private Volume volume;
    private LensDistortion lensDistortion;

    void Awake()
    {
        currentPositions = AreaPositions[0].PlayerPositions;
        currentPosition = currentPositions.IndexOf(AreaPositions[0].StartPos);
        CurrentVeroPosition = currentPositions[currentPosition];
        if (enabled)
            ChangeTransform();
        //RegisterButtonEvents();

        //volume = FindObjectOfType<Volume>();
        //volume.profile.TryGet(out lensDistortion);
    }

    private void OnEnable()
    {
        XRControls.Instance.ControllerEventButton -= Teleport;
        XRControls.Instance.ControllerEventButton += Teleport;
    }

    private void OnDisable()
    {
        XRControls.Instance.RemoveButtonEvents();
    }



    public void Teleport(bool left)
    {
        
        // WMR Headset
        if (!GameManagerVR.Instance.OculusInUse)
        {
            float offsetTouch = 0.4f;
            GameManagerVR.Instance.RightCon.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightTouchpad);

            if (rightTouchpad.y < -offsetTouch)
                left = false;
            else if (rightTouchpad.y > offsetTouch)
                left = true;
            else
                return;
        }

        if (!left)
        {
            currentPosition++;
            if (currentPosition > currentPositions.Count - 1)
                currentPosition = 0;
        }
        else
        {
            currentPosition--;
            if (currentPosition < 0)
                currentPosition = currentPositions.Count - 1;
        }

        ChangeTransform();
    }

    private void ChangeTransform()
    {
        transform.position = currentPositions[currentPosition].position;
        transform.rotation = currentPositions[currentPosition].localRotation;
        CurrentVeroPosition = currentPositions[currentPosition];
    }

    public void ChangeArea(string name)
    {

    }

    public void RecenterPlayer()
    {

    }

    public async void MoveVRPlayer(GameObject goal, float duration)
    {
        _ = PositionManager.Instance.VisualizeSceneChange(true);
        await MoveIntoCharacter(goal.transform, duration);
        _ = PositionManager.Instance.VisualizeSceneChange(false);
    }

    public async Task MoveIntoCharacter(Transform goal, float timeToMove)
    {
        float distance = Vector3.Distance(transform.position, goal.position);
        float deltaToMove = distance / timeToMove;

        while (distance > 0.5f)
        {
            distance = Vector3.Distance(transform.position, goal.position);
            transform.position = Vector3.MoveTowards(transform.position, goal.position, Time.deltaTime * deltaToMove);
            await Task.Yield();
        }
        goal.gameObject.SetActive(false);
        transform.position = goal.position;
        ToggleControllerOrHands(true, false);
    }

    public void ToggleControllerOrHands(bool activateHands, bool holdingObject)
    {
        HandPresence[] hands = FindObjectsOfType<HandPresence>();
        hands[0].ToggleHands(activateHands);
        hands[1].ToggleHands(activateHands);

        if (holdingObject)
        {
            hands[0].transform.parent.GetComponent<XRDirectInteractor>().
                selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
            hands[1].transform.parent.GetComponent<XRDirectInteractor>().
                selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        }
    }

}

[Serializable]
public class AreaPositions
{
    public string AreaName;
    public Transform StartPos;
    public List<Transform> PlayerPositions;

}
