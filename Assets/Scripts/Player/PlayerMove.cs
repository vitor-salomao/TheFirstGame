using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody hips;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float strafeSpeed = 100f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 75f;
    [SerializeField] private Animator animator;
    private static readonly int WalkSpeedHash = Animator.StringToHash("WalkSpeed");

    private float maxSpeed;

    [Header("Turning")]
    private PlayerLook look;
    private ConfigurableJoint hipsJoint;
    private Quaternion startRotation;
    private float startYaw;
    private bool turnInitialized;

    [Header("GroundCheck")]
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.3f;

    private bool isGrounded;
    private bool jumpQueued;

    private PlayerActions.OnFootActions onFoot;

    private void Awake()
    {
        onFoot = new PlayerActions().OnFoot;
        look = GetComponent<PlayerLook>();
        hipsJoint = hips.GetComponent<ConfigurableJoint>();
        startRotation = hips.transform.rotation;
        maxSpeed = speed * sprintMultiplier;
    }

    private void OnEnable() => onFoot.Enable();
    private void OnDisable() => onFoot.Disable();

    private void Update()
    {
        if (onFoot.Jump.WasPressedThisFrame()) jumpQueued = true;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Turn();
        Move();
    }

    private void CheckGround()
    {
        isGrounded = FootOnGround(leftFoot) || FootOnGround(rightFoot);
    }

    private bool FootOnGround(Transform foot)
    {
        return Physics.Raycast(foot.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void Turn()
    {
        if (!turnInitialized)
        {
            startYaw = look.yaw;
            turnInitialized = true;
        }

        Quaternion yawDelta = Quaternion.Euler(0f, look.yaw - startYaw, 0f);
        hipsJoint.targetRotation = Quaternion.Inverse(startRotation) * Quaternion.Inverse(yawDelta) * startRotation;
    }

    private void Move()
    {
        Walk();
        Jump();
    }

    private void Walk()
    {
        Vector2 input = onFoot.Move.ReadValue<Vector2>();
        Vector3 forward = Vector3.ProjectOnPlane(hips.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(hips.transform.right, Vector3.up).normalized;

        Vector3 direction = forward * (input.y * speed) + right * (input.x * strafeSpeed);
        float sprintMult = (onFoot.Sprint.IsPressed() && isGrounded) ? sprintMultiplier : 1f;

        Vector3 force = sprintMult * direction;
        hips.AddForce(force);

        animator.SetFloat(WalkSpeedHash, (force.magnitude * 1.5f) / maxSpeed);
    }

    private void Jump()
    {
        if (jumpQueued && isGrounded) hips.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);

        jumpQueued = false;
    }
}