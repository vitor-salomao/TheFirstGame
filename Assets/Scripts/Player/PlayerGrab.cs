using System;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Rigidbody holdAnchor;
    [SerializeField] private LayerMask grabMask;
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private float minHoldDistance = 0.6f;

    [Header("Strength")]
    [SerializeField] private float spring = 800f;
    [SerializeField] private float damper = 60f;
    [Tooltip("Max force (N) your arms can apply. About 10 N per kg, so 400 = 40 kg.")]
    [SerializeField] private float strength = 400f;

    private PlayerLook look;
    private PlayerActions.OnFootActions onFoot;
    private Collider[] playerColliders;

    private WorldItem held;
    private ConfigurableJoint joint;
    private RigidbodyInterpolation originalInterpolation;
    private float holdDistance;

    public GrabState State { get; private set; }
    public float CarriedMass => held != null ? held.Body.mass / Mathf.Max(1, held.HolderCount) : 0f;

    public event Action<GrabState> StateChanged;

    private void Awake()
    {
        look = GetComponent<PlayerLook>();
        onFoot = new PlayerActions().OnFoot;
        playerColliders = GetComponentsInChildren<Collider>();
        holdDistance = minHoldDistance;
    }

    private void OnEnable() => onFoot.Enable();

    private void OnDisable()
    {
        Release();
        onFoot.Disable();
    }

    private void Update()
    {
        if (held == null) LookForItem();

        if (onFoot.Grab.WasPressedThisFrame()) TryGrab();
        if (onFoot.Grab.WasReleasedThisFrame()) Release();
    }

    private void FixedUpdate()
    {
        holdAnchor.MovePosition(GetHoldPosition());
        holdAnchor.MoveRotation(Quaternion.Euler(0f, look.Yaw, 0f));
    }

    private Vector3 GetHoldPosition()
    {
        return cameraPivot.position + cameraPivot.forward * holdDistance;
    }

    private void LookForItem()
    {
        bool foundItem = TryFindItem(out _, out _);
        SetState(foundItem ? GrabState.CanGrab : GrabState.None);
    }

    private bool TryFindItem(out WorldItem item, out RaycastHit hit)
    {
        item = null;

        bool hitSomething = Physics.Raycast(cameraPivot.position, cameraPivot.forward, out hit, reach, grabMask, QueryTriggerInteraction.Ignore);
        if (!hitSomething) return false;

        item = hit.collider.GetComponentInParent<WorldItem>();
        return item != null && item.IsGrabbable;
    }

    private void TryGrab()
    {
        Debug.Log("Grab pressed");
        if (held != null) return;

        bool foundItem = TryFindItem(out WorldItem item, out RaycastHit hit);
        LogHit(hit);

        if (foundItem) Grab(item, hit);
    }

    private void LogHit(RaycastHit hit)
    {
        if (hit.collider == null)
        {
            Debug.Log("Ray hit nothing");
            return;
        }

        string layer = LayerMask.LayerToName(hit.collider.gameObject.layer);
        Debug.Log($"Ray hit: {hit.collider.name} (layer: {layer})");
    }

    private void Grab(WorldItem item, RaycastHit hit)
    {
        held = item;
        held.AddHolder();

        holdDistance = Mathf.Max(minHoldDistance, hit.distance);
        holdAnchor.position = GetHoldPosition();

        Vector3 grabPoint = held.HoldByCenter ? held.Body.worldCenterOfMass : hit.point;
        joint = CreateJoint(grabPoint);

        originalInterpolation = held.Body.interpolation;
        held.Body.interpolation = RigidbodyInterpolation.Interpolate;
        held.Body.WakeUp();

        IgnoreHeldItem(true);
        SetState(GrabState.Holding);

        Debug.Log($"Grabbed {held.name}");
    }

    private void Release()
    {
        if (held == null) return;

        Debug.Log($"Released {held.name}");

        if (joint != null) Destroy(joint);
        held.Body.interpolation = originalInterpolation;
        held.RemoveHolder();

        IgnoreHeldItem(false);

        joint = null;
        held = null;
        SetState(GrabState.None);
    }

    // A spring that pulls the grabbed point toward the hold anchor, and lets the item swing freely
    private ConfigurableJoint CreateJoint(Vector3 grabPoint)
    {
        ConfigurableJoint newJoint = held.gameObject.AddComponent<ConfigurableJoint>();
        newJoint.connectedBody = holdAnchor;
        newJoint.autoConfigureConnectedAnchor = false;
        newJoint.anchor = held.transform.InverseTransformPoint(grabPoint);
        newJoint.connectedAnchor = Vector3.zero;

        newJoint.xMotion = newJoint.yMotion = newJoint.zMotion = ConfigurableJointMotion.Free;
        newJoint.angularXMotion = newJoint.angularYMotion = newJoint.angularZMotion = ConfigurableJointMotion.Free;

        JointDrive drive = new() { positionSpring = spring, positionDamper = damper, maximumForce = strength };
        newJoint.xDrive = newJoint.yDrive = newJoint.zDrive = drive;

        return newJoint;
    }

    // Stops the held item from pushing the player around
    private void IgnoreHeldItem(bool ignore)
    {
        foreach (Collider itemCollider in held.GetComponentsInChildren<Collider>())
        {
            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider != null) Physics.IgnoreCollision(playerCollider, itemCollider, ignore);
            }
        }
    }

    private void SetState(GrabState newState)
    {
        if (State == newState) return;

        State = newState;
        StateChanged?.Invoke(newState);
    }
}
