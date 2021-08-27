using System.Threading.Tasks;
using UnityEngine;

public class ARInteractionController : MonoBehaviour, IInteractionController
{
    public float rotationSpeed = 10;
    public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }

    private InteractableObject selectedObject;

    // Finger / Joystick positions
    private Vector2 currentPos;
    private Vector2 lastPos;
    private Vector2 deltaPos;
    // When using 2 fingers
    private Vector2 currentPos2;
    private Vector2 lastPos2;
    private Vector2 deltaPo2;

    private bool holdingFingerOnObj;

    // For dragging objects 
    private Vector3 startPos;
    private Vector3 newPos;



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
                // When there was an object selected already
                if (selectedObject)
                    selectedObject.SelectionReticle.SetActive(false);

                selectedObject = hit.transform.GetComponent<InteractableObject>();
                selectedObject.SelectionReticle.SetActive(true);
                holdingFingerOnObj = true;

                // For moving
                startPos = selectedObject.transform.position;
                Vector3 dist = Camera.main.WorldToScreenPoint(startPos);
                newPos.x = Input.mousePosition.x - dist.x;
                newPos.y = Input.mousePosition.y - dist.y;
                newPos.z = Input.mousePosition.z - dist.z;
            }
        }
    }

    private void FingerHeld()
    {
        if (selectedObject == null) return;

        currentPos = Input.GetTouch(0).position;
        deltaPos = Input.GetTouch(0).deltaPosition;
        lastPos = currentPos - deltaPos;

        // Moving
        if (holdingFingerOnObj && Mathf.Abs(deltaPos.magnitude) > 10 && Input.touchCount == 1)
        {
            MoveObject();
        }

        // Scaling
        if (Input.touchCount >= 2 && holdingFingerOnObj)
        {
            ScaleObject();
        }

        // Rotation
        if (!holdingFingerOnObj)
        {
            RotateObject();
        }
    }

    private async void FingerReleased()
    {
        await Task.Delay(300);
        if (selectedObject)
        {
            selectedObject.ClientRotates = false;
            selectedObject.ClientScales = false;
            selectedObject.ClientMoves = false;
        }

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
        currentPos2 = Input.GetTouch(1).position;
        lastPos2 = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
        float zoom = Vector3.Distance(currentPos, currentPos2) / Vector3.Distance(lastPos, lastPos2);
        selectedObject.ClientScales = true;

        // Check if objects is in desired scale range
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
        Vector3 dis = new Vector3(
        Input.mousePosition.x - newPos.x,
        Input.mousePosition.y - newPos.y,
        Input.mousePosition.z - newPos.z
        );

        Vector3 lastPos = Camera.main.ScreenToWorldPoint(new Vector3(dis.x, dis.y, dis.z));
        selectedObject.transform.position = new Vector3(lastPos.x, startPos.y, lastPos.z);
    }
}
