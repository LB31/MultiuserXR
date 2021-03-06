using MLAPI;
using System.Threading.Tasks;
using UnityEngine;

public class ARInteractionController : InteractionController
{
    public float rotationSpeed = 10;
    public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }

    public LayerMask LayerMask;

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

    public void FingerPressed()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.value))
        {
            bool selected = SelectObject(hit.transform.gameObject);

            holdingFingerOnObj = selected;
            deselectedWithoutInteraction = selected;
            Debug.Log("selected " + hit.transform.gameObject.name);
        }

        // For moving
        previousMousePos = Input.mousePosition;
        prevCamRotation = Camera.main.transform.eulerAngles;
    }

    private void FingerHeld()
    {
        if (selectedObject == null || selectedObject.SelectedBy.Value != NetworkManager.Singleton.LocalClientId) return;

        currentPos = Input.mousePosition;
        deltaPos = Input.mousePosition - previousMousePos;
        previousMousePos = currentPos;

        lastPos = currentPos - deltaPos;

        int touchCount = Input.touchCount;
#if UNITY_EDITOR
        touchCount = 1;
#endif
        //float distanceAngle = Vector3.Angle(Camera.main.transform.eulerAngles, prevCamRotation);

        // Moving; only allowed with one finger + when finger was moved or when cam has rotated enough
        if (holdingFingerOnObj && touchCount == 1 && (Mathf.Abs(deltaPos.magnitude) > 10 || Vector3.Angle(Camera.main.transform.eulerAngles, prevCamRotation) > 3))
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
        else if (!holdingFingerOnObj && touchCount == 1)
        {
            RotateObject();
            deselectedWithoutInteraction = false;
        }

        prevCamRotation = Camera.main.transform.eulerAngles;
    }

    private void FingerReleased()
    {
        if (!selectedObject) return;
        bool wholeDeselect = selectedObject.Equals(previousSelectedObject) && deselectedWithoutInteraction;
        DeselectObject(wholeDeselect);
        deselectedWithoutInteraction = !wholeDeselect;
        holdingFingerOnObj = false;
    }

    public void RotateObject()
    {
        float rotation = (lastPos - currentPos).x;
        selectedObject.ClientRotates = true;
        selectedObject.transform.Rotate(Vector3.up, rotation * Time.deltaTime * RotationSpeed);
    }

    public void ScaleObject()
    {
        if (!selectedObject.ScalingAllowed) return;

        currentPos2 = Input.GetTouch(1).position;
        lastPos2 = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
        float zoom = Vector3.Distance(currentPos, currentPos2) / Vector3.Distance(lastPos, lastPos2);
        selectedObject.ClientScales = true;

        // Check if object is in desired scale range
        Vector3 expectedScale = selectedObject.transform.localScale * zoom;
        if (expectedScale.y > selectedObject.AllowedMin && expectedScale.y < selectedObject.AllowedMax)
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
