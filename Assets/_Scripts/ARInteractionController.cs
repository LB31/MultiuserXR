using System.Threading.Tasks;
using UnityEngine;

public class ARInteractionController : InteractionController
{
    protected InteractableObject selectedObject;

    // Finger / Joystick positions
    protected Vector2 currentPos;
    protected Vector2 lastPos;
    protected Vector2 deltaPos;
    // When using 2 fingers
    protected Vector2 currentPos2;
    protected Vector2 lastPos2;
    protected Vector2 deltaPo2;

    private bool holdingFingerOnObj;

    private void Update()
    {
        SetTransform();
    }

    private async void SetTransform()
    {
        // When finger was pressed
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
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
                }
            }
        }

        // When finger / mouse releases the screen
        if (Input.GetMouseButtonUp(0))
        {
            await Task.Delay(300);
            if (selectedObject)
            {
                selectedObject.ClientRotates = false;
                selectedObject.ClientScales = false;
            }


            holdingFingerOnObj = false;
        }

        // Holding finger
        if (Input.GetMouseButton(0))
        {
            if (selectedObject == null) return;

            currentPos = Input.GetTouch(0).position;
            lastPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;

            // Scaling
            if (Input.touchCount >= 2 && holdingFingerOnObj)
            {
                currentPos2 = Input.GetTouch(1).position;
                lastPos2 = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
                float zoom = Vector3.Distance(currentPos, currentPos2) / Vector3.Distance(lastPos, lastPos2);
                selectedObject.ClientScales = true;

                ScaleObject(selectedObject, zoom);

            }

            // Rotation
            float rotation = (lastPos - currentPos).x;
            selectedObject.ClientRotates = true;
            RotateObject(selectedObject.transform, rotation);

        }
    }
}
