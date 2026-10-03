using System;
using UnityEngine;
using UnityPipeline.Microsoft.CodeAnalysis.CSharp.Syntax;

public class PlayerController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody hips;
    [SerializeField] private float speed = 150f;
    [SerializeField] private float strafeSpeed = 100f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 75f;

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

        float sprintMult = onFoot.Sprint.IsPressed() ? sprintMultiplier : 1f;
        hips.AddForce(sprintMult * direction);
    }

    private void Jump()
    {
        if (jumpQueued && isGrounded) hips.AddForce(jumpForce * hips.transform.up, ForceMode.Impulse);

        jumpQueued = false;
    }
}
