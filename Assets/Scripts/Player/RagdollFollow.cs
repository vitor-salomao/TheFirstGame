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
    }

    private void OnEnable() => move.Jumped += OnJumped;
    private void OnDisable() => move.Jumped -= OnJumped;

    private void FixedUpdate()
    {
        Turn();
        Follow();
    }

    private void OnJumped()
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

        Vector3 rootVelocity = root.linearVelocity;
        Vector3 hipsVelocity = hips.linearVelocity;
        Vector3 velocityDiff = new(rootVelocity.x - hipsVelocity.x, 0f, rootVelocity.z - hipsVelocity.z);

        hips.AddForce(distanceToTarget * followSpring + velocityDiff * followDamping, ForceMode.Acceleration);

        float planarSpeed = new Vector3(rootVelocity.x, 0f, rootVelocity.z).magnitude;
        animator.SetFloat(WalkSpeedHash, planarSpeed / move.MaxSpeed);
    }
}