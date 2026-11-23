using UnityEngine;

public class RagdollFollow : MonoBehaviour
{
    [SerializeField] private Rigidbody root;
    [SerializeField] private Rigidbody hips;
    [SerializeField] private Animator animator;
    [SerializeField] private float followSpring = 100f;
    [SerializeField] private float followDamping = 20f;

    private static readonly int WalkSpeedHash = Animator.StringToHash("WalkSpeed");

    private PlayerLook look;
    private PlayerMove move;
    private ConfigurableJoint hipsJoint;
    private Quaternion startRotation;
    private Vector3 positionOffset;
    private float startYaw;
    private float totalMass;
    private Vector3 lastRootPosition;
    private Vector3 measuredVelocity;

    private void Awake()
    {
        look = root.GetComponent<PlayerLook>();
        startYaw = look.Yaw;

        move = root.GetComponent<PlayerMove>();

        hipsJoint = hips.GetComponent<ConfigurableJoint>();
        startRotation = hips.transform.rotation;

        positionOffset = hips.position - root.position;
        positionOffset.y = 0f;

        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>()) totalMass += body.mass;
        lastRootPosition = root.transform.position;
    }

    private void OnEnable() => move.Jumped += Jump;
    private void OnDisable() => move.Jumped -= Jump;

    private void FixedUpdate()
    {
        Turn();
        Follow();
    }

    // Public so a jump from another player (sent over the network) can play on this body too
    public void Jump()
    {
        float rootJumpVelocity = Mathf.Sqrt(2f * -Physics.gravity.y * move.JumpHeight);
        hips.AddForce(rootJumpVelocity * totalMass * Vector3.up, ForceMode.Impulse);
    }

    private void Turn()
    {
        Quaternion yawDelta = Quaternion.Euler(0f, look.Yaw - startYaw, 0f);
        hipsJoint.targetRotation = Quaternion.Inverse(startRotation) * Quaternion.Inverse(yawDelta) * startRotation;
    }

    private void Follow()
    {
        Vector3 target = root.position + Quaternion.Euler(0f, look.Yaw - startYaw, 0f) * positionOffset;
        Vector3 distanceToTarget = target - hips.position;
        distanceToTarget.y = 0f;

        Vector3 rootVelocity = GetRootVelocity();
        Vector3 hipsVelocity = hips.linearVelocity;
        Vector3 velocityDiff = new(rootVelocity.x - hipsVelocity.x, 0f, rootVelocity.z - hipsVelocity.z);

        hips.AddForce(distanceToTarget * followSpring + velocityDiff * followDamping, ForceMode.Acceleration);

        float planarSpeed = new Vector3(rootVelocity.x, 0f, rootVelocity.z).magnitude;
        animator.SetFloat(WalkSpeedHash, planarSpeed / move.MaxSpeed);
    }

    // Your own body moves with physics, so its velocity is real. Other players' bodies are moved by the network,
    // so their physics velocity is always 0 and we measure how far they moved instead.
    private Vector3 GetRootVelocity()
    {
        Vector3 position = root.transform.position;
        Vector3 moved = (position - lastRootPosition) / Time.fixedDeltaTime;
        lastRootPosition = position;

        if (!root.isKinematic) return root.linearVelocity;

        // Network updates don't line up with physics steps, so smooth out the jumps between them
        measuredVelocity = Vector3.Lerp(measuredVelocity, moved, 0.25f);
        return measuredVelocity;
    }
}