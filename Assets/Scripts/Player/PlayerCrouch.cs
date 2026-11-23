using UnityEngine;

// Hold crouch to lower the camera and shrink the collider, so you can reach low items
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCrouch : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [Tooltip("How far down you go, in meters.")]
    [SerializeField] private float crouchDepth = 1f;
    [Tooltip("How fast you go down and up, in meters per second.")]
    [SerializeField] private float crouchSpeed = 4f;

    private PlayerActions.OnFootActions onFoot;
    private CapsuleCollider capsule;
    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingPivot;
    private float currentDepth;

    public bool IsCrouching => currentDepth > 0f;

    private void Awake()
    {
        onFoot = new PlayerActions().OnFoot;
        capsule = GetComponent<CapsuleCollider>();

        standingHeight = capsule.height;
        standingCenter = capsule.center;
        standingPivot = cameraPivot.localPosition;
    }

    private void OnEnable() => onFoot.Enable();
    private void OnDisable() => onFoot.Disable();

    private void Update()
    {
        float targetDepth = onFoot.Crouch.IsPressed() ? crouchDepth : 0f;
        currentDepth = Mathf.MoveTowards(currentDepth, targetDepth, crouchSpeed * Time.deltaTime);

        ApplyDepth(ToLocal(currentDepth));
    }

    // Lowers the top of the body while the feet stay on the ground
    private void ApplyDepth(float depth)
    {
        cameraPivot.localPosition = standingPivot + Vector3.down * depth;
        capsule.height = standingHeight - depth;
        capsule.center = standingCenter + Vector3.down * depth / 2f;
    }

    // The player object is scaled, so meters have to be converted to its local units
    private float ToLocal(float meters)
    {
        return meters / transform.lossyScale.y;
    }
}
