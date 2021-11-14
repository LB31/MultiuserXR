using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour
{
    public float Speed = 1;
    //public XRNode InputSource;
    public float AdditionalHeight = 0.2f;

    private XRRig rig;
    private Vector2 inputAxis;
    private CharacterController character;
    private float gravity = -9.81f;
    private float fallingSpeed;

    private void OnEnable()
    {
        //XRControls.Instance.ControllerEventTrigger -= Interact;
        //XRControls.Instance.ControllerEventTrigger += Interact;
    }

    private void OnDisable()
    {
        // TODO test if this works
        //XRControls.Instance.ControllerEventTrigger -= Interact;
    }

    void Start()
    {
        rig = GetComponent<XRRig>();
        character = GetComponent<CharacterController>();
    }

    void Update()
    {
        VRManager.Instance.LeftCon.TryGetFeatureValue(VRManager.Instance.Axis2D, out inputAxis);
    }

    private void FixedUpdate()
    {
        CapsuleFollowHeadset();

        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);

        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);

        if (inputAxis.magnitude > 0.15f)
            character.Move(Speed * Time.fixedDeltaTime * direction);

        // gravity 
        if (character.isGrounded)
            fallingSpeed = 0;
        else
            fallingSpeed += gravity * Time.fixedDeltaTime;

        character.Move(fallingSpeed * Time.fixedDeltaTime * Vector3.up);
    }

    private void CapsuleFollowHeadset()
    {
        character.height = rig.cameraInRigSpaceHeight + AdditionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        character.center = new Vector3(capsuleCenter.x, character.height * 0.5f + character.skinWidth, capsuleCenter.z);
    }

    public void Interact()
    {
    }

}
