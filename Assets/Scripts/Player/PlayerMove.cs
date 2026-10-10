using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private PlayerLook look;
    private PlayerActions.OnFootActions onFoot;
    private bool isGrounded;
    private bool jumpQueued;

    public bool IsGrounded => isGrounded;
    public float MaxSpeed => walkSpeed * sprintMultiplier;
    public float JumpHeight => jumpHeight;

    public event System.Action Jumped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PlayerLook>();
        onFoot = new PlayerActions().OnFoot;
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

        Vector3 velocity = rb.linearVelocity;
        velocity = Move(velocity);
        velocity = Jump(velocity);
        rb.linearVelocity = velocity;
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private Vector3 Move(Vector3 velocity)
    {
        Vector2 input = onFoot.Move.ReadValue<Vector2>();
        Vector3 wishDir = Quaternion.Euler(0f, look.Yaw, 0f) * new Vector3(input.x, 0f, input.y);
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);

        float speed = walkSpeed * (onFoot.Sprint.IsPressed() && isGrounded ? sprintMultiplier : 1f);

        Vector3 planar = new(velocity.x, 0f, velocity.z);
        float accel = acceleration * (isGrounded ? 1f : airControl);
        planar = Vector3.MoveTowards(planar, wishDir * speed, accel * Time.fixedDeltaTime);

        velocity.x = planar.x;
        velocity.z = planar.z;
        return velocity;
    }

    private Vector3 Jump(Vector3 velocity)
    {
        if (jumpQueued && isGrounded)
        {
            velocity.y = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
            Jumped?.Invoke();
        }

        jumpQueued = false;
        return velocity;
    }
}