using MLAPI;
using System.Threading.Tasks;
using UnityEngine;

public class ARInteractionController : MonoBehaviour, IInteractionController
{
    public float rotationSpeed = 10;
    public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }

    private InteractableObject selectedObject;
    private InteractableObject previousSelectedObject;

    // Finger / Joystick positions
    private Vector2 currentPos;
    private Vector2 lastPos;
    private Vector2 deltaPos;
    // When using 2 fingers
    private Vector2 currentPos2;
    private Vector2 lastPos2;
    private Vector2 deltaPo2;

    private bool holdingFingerOnObj;
    private bool deselectedWithoutInteraction;

    // For dragging objects 
    private Vector3 startPos;
    private Vector3 newPos;

    private Vector3 previousMousePos;
    private Vector3 prevCamRotation;


    private void Update()
    {
        SetTransform();
    }

    private void SetTransform()
    {
        // When finger was pressed
        if (Input.GetMouseButtonDown(0))
            FingerPressed();

        // When finger / mouse releases the screen
        if (Input.GetMouseButtonUp(0))
            FingerReleased();

        // Holding finger
        if (Input.GetMouseButton(0))
            FingerHeld();
    }

    private void FingerPressed()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // only hit objects that are shared through network
            if (hit.transform.GetComponent<InteractableObject>())
            {
                previousSelectedObject = selectedObject;
                selectedObject = hit.transform.GetComponent<InteractableObject>();
                selectedObject.SelectionReticle.SetActive(true);

                // Is this object selected already by someone else?
                if (selectedObject.SelectedBy.Value != ulong.MaxValue &&
                    selectedObject.SelectedBy.Value != NetworkManager.Singleton.LocalClientId)
                {
                    selectedObject = null;
                    return;
                }
                // Show that the other object was selected by Client now
                else
                {
                    selectedObject.SelectedBy.Value = NetworkManager.Singleton.LocalClientId;
                    selectedObject.SelectionReticle.GetComponent<SpriteRenderer>().color = Color.cyan;
                }

                // When Client selects a new object
                if (previousSelectedObject && previousSelectedObject != selectedObject)
                {
                    previousSelectedObject.SelectionReticle.SetActive(false);
                    previousSelectedObject.SelectedBy.Value = ulong.MaxValue;
                }

                holdingFingerOnObj = true;
                deselectedWithoutInteraction = true;
             
            }
        }

        // For moving
        previousMousePos = Input.mousePosition;
        prevCamRotation = Camera.main.transform.eulerAngles;
    }

    private void FingerHeld()
    {
        if (selectedObject == null) return;


        currentPos = Input.mousePosition;
        deltaPos = Input.mousePosition - previousMousePos;
        previousMousePos = currentPos;

        lastPos = currentPos - deltaPos;

        int touchCount = Input.touchCount;
#if UNITY_EDITOR
        touchCount = 1;
#endif
        float distanceAngle = Vector3.Angle(Camera.main.transform.eulerAngles, prevCamRotation);

        Debug.Log(distanceAngle > 2 ? distanceAngle : 1);

        // Moving; only allowed with one finger + when finger was moved or when cam has rotated enough
        if (holdingFingerOnObj && Input.touchCount == 1 && (Mathf.Abs(deltaPos.magnitude) > 10 || Vector3.Angle(Camera.main.transform.eulerAngles, prevCamRotation) > 3))
        {
            MoveObject();
            deselectedWithoutInteraction = false;
        }
        // Scaling
        else if (holdingFingerOnObj && Input.touchCount >= 2)
        {
            ScaleObject();
            deselectedWithoutInteraction = false;
        }
        // Rotation
        else if (!holdingFingerOnObj && Input.touchCount == 1)
        {
            RotateObject();
            deselectedWithoutInteraction = false;
        }

        prevCamRotation = Camera.main.transform.eulerAngles;
    }

    private async void FingerReleased()
    {
        await Task.Delay(300);
        if (selectedObject)
        {
            selectedObject.ClientRotates = false;
            selectedObject.ClientScales = false;
            selectedObject.ClientMoves = false;
            holdingFingerOnObj = false;

            // Deselect when the same object was hit without interaction
            if (selectedObject.Equals(previousSelectedObject) && deselectedWithoutInteraction)
            {
                selectedObject.SelectedBy.Value = ulong.MaxValue;
                selectedObject.SelectionReticle.SetActive(false);
                selectedObject = null;
                deselectedWithoutInteraction = false;
            }
        }
    }

    public void RotateObject()
    {
        float rotation = (lastPos - currentPos).x;
        selectedObject.ClientRotates = true;
        selectedObject.transform.Rotate(Vector3.up, rotation * Time.deltaTime * RotationSpeed);
    }

    public void ScaleObject()
    {
        currentPos2 = Input.GetTouch(1).position;
        lastPos2 = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
        float zoom = Vector3.Distance(currentPos, currentPos2) / Vector3.Distance(lastPos, lastPos2);
        selectedObject.ClientScales = true;

        // Check if object is in desired scale range
        Vector3 expectedScale = selectedObject.transform.localScale * zoom;
        if (expectedScale.y > selectedObject.allowedMin && expectedScale.y < selectedObject.allowedMax)
        {
            selectedObject.transform.localScale *= zoom;
            if (selectedObject.PivotIsInMiddle)
            {
                Vector3 curPos = selectedObject.transform.localPosition;
                selectedObject.transform.localPosition = new Vector3(curPos.x, curPos.y - selectedObject.GetHeight() * 0.5f, curPos.z);
            }
        }
    }

    
    public void MoveObject()
    {
        selectedObject.ClientMoves = true;

        // Layer mask named DraggingPlane
        int layerMask = 1 << 6;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // If point on plane was hit
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            selectedObject.transform.position = hit.point;
        }

        // http://answers.unity.com/answers/1215311/view.html
        //Vector3 dis = new Vector3(
        //Input.mousePosition.x - newPos.x,
        //Input.mousePosition.y - newPos.y,
        //Input.mousePosition.z - newPos.z
        //);
        //Vector3 lastPos = Camera.main.ScreenToWorldPoint(new Vector3(dis.x, dis.y, dis.z));
        //selectedObject.transform.position = new Vector3(lastPos.x, startPos.y, lastPos.z);
    }
}
