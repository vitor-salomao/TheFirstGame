using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Rigidbody holdAnchor;
    [SerializeField] private LayerMask grabMask;
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private float holdDistance = 1.2f;

    [Header("Strength")]
    [SerializeField] private float spring = 800f;
    [SerializeField] private float damper = 60f;
    [SerializeField] private float maxForce = 400f;

    private PlayerLook look;
    private PlayerActions.OnFootActions onFoot;
    private ConfigurableJoint joint;
    private Rigidbody held;

    public float CarriedMass => held != null ? held.mass : 0f;

    private void Awake()
    {
        look = GetComponent<PlayerLook>();
        onFoot = new PlayerActions().OnFoot;
    }

    private void OnEnable() => onFoot.Enable();
    private void OnDisable() => onFoot.Disable();

    private void Update()
    {
        if (onFoot.Grab.WasPressedThisFrame()) TryGrab();
        if (onFoot.Grab.WasReleasedThisFrame()) Release();
    }

    private void FixedUpdate()
    {
        holdAnchor.MovePosition(cameraPivot.position + cameraPivot.forward * holdDistance);
        holdAnchor.MoveRotation(Quaternion.Euler(0f, look.Yaw, 0f));
    }

    private void TryGrab()
    {
        if (held != null) return;
        if (!Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, reach, grabMask)) return;
        if (hit.rigidbody == null) return;

        held = hit.rigidbody;

        joint = held.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = holdAnchor;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
        joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;

        JointDrive linear = new() { positionSpring = spring, positionDamper = damper, maximumForce = maxForce };
        joint.xDrive = joint.yDrive = joint.zDrive = linear;

        joint.rotationDriveMode = RotationDriveMode.Slerp;
        joint.slerpDrive = new JointDrive { positionSpring = spring * 0.25f, positionDamper = damper * 0.5f, maximumForce = maxForce };
    }

    private void Release()
    {
        if (joint != null) Destroy(joint);
        joint = null;
        held = null;
    }
}