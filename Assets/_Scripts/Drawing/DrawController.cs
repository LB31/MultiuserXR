using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawController : MonoBehaviour
{
    public MainPlatform CurrentPlatform;
    public LayerMask Drawing_Layers;
    public float MaxDistanceToBoard = 1;

    private DrawManager dm;
    private bool noDrawingOnCurrentDrag = false;

    [Header("VR Information")]
    public Transform BrushTip;
    public float DistancteToDraw = 0.2f;

    public XRInput LeftHandInput;
    public XRInput RightHandInput;

    private List<XRBinding> bindingsRight = new List<XRBinding>();

    private void Start()
    {
        dm = DrawManager.drawable;
    }

    private void OnEnable()
    {
        // For debugging: only assign platform value if in build
        if (CurrentPlatform.Equals(MainPlatform.UNSPECIFIED))
            CurrentPlatform = GameManager.Instance.CurrentPlatform;
        dm = DrawManager.drawable;

        AssignDrawButttons();
    }

    private void Update()
    {
        if (Camera.main == null || dm == null) return;

        // Check if user is near enough to draw
        float distance = Vector3.Distance(Camera.main.transform.position, dm.transform.position);
        if (distance > MaxDistanceToBoard) return;

        if (CurrentPlatform == MainPlatform.MOBILE || CurrentPlatform == MainPlatform.DESKTOP)
            TrackInputAR();
        //if (CurrentPlatform == MainPlatform.VR_WINDOWS || CurrentPlatform == MainPlatform.VR_ANDROID)
        //    TrackInputVR();
    }

    private void AssignDrawButttons()
    {
        if(bindingsRight.Count == 0)
        {
            bindingsRight.Add(new XRBinding(XRButton.Trigger, PressType.Continuous, () => TrackInputVR()));
            bindingsRight.Add(new XRBinding(XRButton.Trigger, PressType.End, () => Deselect()));
            foreach (var item in bindingsRight)
                RightHandInput.Bindings.Add(item);
        } 
    }

    public void TrackInputVR()
    {
        Vector3 fwd = BrushTip.TransformDirection(Vector3.forward);

        if (Physics.Raycast(BrushTip.position, fwd, out RaycastHit hit, DistancteToDraw, Drawing_Layers.value))
            RayHit(hit);
        // not over destination texture
        else
        {
            Deselect();
        }
    }

    private void TrackInputAR()
    {
        // Holding mouse / finger
        if (Input.GetMouseButton(0) && !noDrawingOnCurrentDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Drawing_Layers.value))
                RayHit(hit);
            // not over destination texture
            else
            {
                dm.PreviousDragPosition = Vector2.zero;
                noDrawingOnCurrentDrag = true;
            }
        }

        // Release mouse / finger
        if (Input.GetMouseButtonUp(0))
        {
            Deselect();
        }
    }

    public void Deselect()
    {
        noDrawingOnCurrentDrag = false;
        dm.PreviousDragPosition = Vector2.zero;
    }

    private void RayHit(RaycastHit hit)
    {
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= dm.Width;
        pixelUV.y *= dm.Height;
        dm.PenBrush(pixelUV);
    }
}
