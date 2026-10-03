using System;
using UnityEngine;
using UnityEngine.InputSystem; // NEW CHANGE: needed to read the mouse via Mouse.current

public class PlayerController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody hips;
    [SerializeField] private float speed = 150f;
    [SerializeField] private float strafeSpeed = 100f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 75f;

    // NEW CHANGE: mouse look setting
    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f; // degrees per pixel of mouse movement


    [Header("GroundCheck")]
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.3f;

    private ConfigurableJoint hipsJoint; // NEW CHANGE: the joint on the hips that pulls the body back to "facing front"
    private float targetYaw;             // NEW CHANGE: accumulated mouse yaw, used as the joint's new target
    private Rigidbody[] bodies; // NEW CHANGE: every Rigidbody in the player, so the whole body turns together
    private bool isGrounded;
    private bool jumpQueued;
    private float pendingYawDelta; // NEW CHANGE: mouse turning stored in Update, applied in FixedUpdate

    private PlayerActions.OnFootActions onFoot;

    private void Awake()
    {
        onFoot = new PlayerActions().OnFoot;
        bodies = GetComponentsInChildren<Rigidbody>();
        hipsJoint = hips.GetComponent<ConfigurableJoint>(); // NEW CHANGE
    }

    private void OnEnable()
    {
        onFoot.Enable();
        Cursor.lockState = CursorLockMode.Locked; // NEW CHANGE: lock and hide cursor
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        onFoot.Disable();
        Cursor.lockState = CursorLockMode.None;   // NEW CHANGE: release cursor
        Cursor.visible = true;
    }

    private void Update()
    {
        if (onFoot.Jump.WasPressedThisFrame()) jumpQueued = true;

        // NEW CHANGE: accumulate horizontal mouse movement
        if (Mouse.current != null)
            pendingYawDelta += Mouse.current.delta.ReadValue().x * mouseSensitivity;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Turn(); // NEW CHANGE: rotate the player toward the mouse direction
        Move();
    }

    // NEW CHANGE: rotates every body part around the hips as one rigid unit,
    // so the joints stay satisfied and nothing pulls the hips back.
    private void Turn()
    {
        if (Mathf.Approximately(pendingYawDelta, 0f)) return;

        Quaternion q = Quaternion.AngleAxis(pendingYawDelta, Vector3.up);
        Vector3 pivot = hips.position;

        // NEW CHANGE: move the joint's target to the new facing so it stops pulling the body back.
        // Unity's targetRotation is inverted, hence Quaternion.Inverse.
        targetYaw += pendingYawDelta;
        hipsJoint.targetRotation = Quaternion.Inverse(Quaternion.Euler(0f, targetYaw, 0f));

        pendingYawDelta = 0f;

        foreach (Rigidbody rb in bodies)
        {
            rb.position = pivot + q * (rb.position - pivot);
            rb.rotation = q * rb.rotation;
            rb.linearVelocity = q * rb.linearVelocity;
            rb.angularVelocity = q * rb.angularVelocity;
        }
    }

    private void CheckGround()
    {
        isGrounded = FootOnGround(leftFoot) || FootOnGround(rightFoot);
    }

    private bool FootOnGround(Transform foot)
    {
        return Physics.Raycast(foot.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
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
        hips.AddForce(sprintMult * direction);
    }

    private void Jump()
    {
        if (jumpQueued && isGrounded) hips.AddForce(jumpForce * hips.transform.up, ForceMode.Impulse);

        jumpQueued = false;
    }
}