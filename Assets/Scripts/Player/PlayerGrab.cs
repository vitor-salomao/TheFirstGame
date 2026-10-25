using System;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{

    // Nothing uses this for now. But it would be good if we want an icon to appear for grabbing, or to walk slower while grabbing
    public enum GrabState { None, CanGrab, Holding }
    public GrabState State { get; private set; }
    public event Action<GrabState> StateChanged;

    [Header("Settings")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Rigidbody holdAnchor;
    [SerializeField] private LayerMask grabMask; 
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private float minHoldDistance = 0.6f; 

    [Header("Strength")]
    [SerializeField] private float spring = 800f;
    [SerializeField] private float damper = 60f;
    [Tooltip("Max force (N) your arms can apply. ~10 N per kg you can lift. 400 = ~40 kg. Was called maxForce before")]
    [SerializeField] private float strength = 400f;

    private PlayerLook look;
    private PlayerActions.OnFootActions onFoot;
    private ConfigurableJoint joint;
    private WorldItem held; // was rigidbody before, but we need the Item component to check if it's grabbable. Its in the item script
    private float holdDistance; 
    private Collider[] playerColliders;
    private Collider[] heldColliders;
    private RigidbodyInterpolation heldPrevInterp;

    /// The total mass of the item that is being held is split between the people who are trying to carry it
    public float CarriedMass => held != null ? held.Body.mass / Mathf.Max(1, held.HolderCount) : 0f;


    private void Awake()
    {
        look = GetComponent<PlayerLook>();
        onFoot = new PlayerActions().OnFoot;
        playerColliders = GetComponentsInChildren<Collider>(); ///this is so the items dont go through the player when grabbed. We will ignore collisions between the player and the held item.
        holdDistance = minHoldDistance;
    }

    private void OnEnable() => onFoot.Enable();

    // the disable will call the item release function,
    private void OnDisable()
    {
        Release();
        onFoot.Disable();
    }

    private void Update()
    {
        Debug.DrawRay(cameraPivot.position, cameraPivot.forward * reach, Color.green); 
        // doesnt do anything for now, just changes the grab state so we can show an icon or something.
        // If the player is holding an item, the state is always holding.
        // If not, it checks if there is a grabbable item in front of the player and sets the state to can grab if so.
        if (held == null)
            SetState(TryFindItem(out _, out _) ? GrabState.CanGrab : GrabState.None);

        if (onFoot.Grab.WasPressedThisFrame()) TryGrab();
        if (onFoot.Grab.WasReleasedThisFrame()) Release();
    }

    private void FixedUpdate()
    {
        holdAnchor.MovePosition(cameraPivot.position + cameraPivot.forward * holdDistance);
        holdAnchor.MoveRotation(Quaternion.Euler(0f, look.Yaw, 0f));
    }

    // new function that checks if there is a grabbable item in front of the player. If so, it returns true and sets the out
    // parameters to the item and the hit info. If not, it returns false and sets the out parameters to null and
    // default.
    private bool TryFindItem(out WorldItem item, out RaycastHit hit)
    {
        item = null;
        if (!Physics.Raycast(cameraPivot.position, cameraPivot.forward, out hit, reach, grabMask,
                QueryTriggerInteraction.Ignore))
            return false;
        item = hit.collider.GetComponentInParent<WorldItem>();
        return item != null && item.IsGrabbable;
    }

    private void TryGrab()
    {
        //this is for debugging purposes, to see if the raycast is hitting anything and what it is hitting.
        Debug.Log("Grab pressed");
        bool rayHit = Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit dbg, reach, grabMask, QueryTriggerInteraction.Ignore);
        Debug.Log(rayHit
            ? $"Ray hit: {dbg.collider.name} (layer: {LayerMask.LayerToName(dbg.collider.gameObject.layer)})"
            : "Ray hit nothing");

        if (held != null) return;
        if (!TryFindItem(out WorldItem item, out RaycastHit hit)) return;

        // this was what we had before and it grabbed anything with a rigidbody, not just Items. 
        // I was thinking for interactables, it could be like "Press E to interract" and not grab. But we can change it
        //if (!Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, reach, grabMask)) return;
        //if (hit.rigidbody == null) return;

        held = item;
        held.AddHolder();

        // Hold it at the distance you grabbed it from, not a fixed distance. We can change this so that its a fixed distance
        // but i thought it was a good idea to add.
        holdDistance = Mathf.Max(minHoldDistance, hit.distance);

        // So basically, from what I understand, this sets the position of the holdAnchor to be where the player is looking,
        // and then it creates a joint between the holdAnchor and the held item. This is so that if it is a really heavy object, 
        // the physics will tilt it properly and not make it just float.
        holdAnchor.position = cameraPivot.position + cameraPivot.forward * holdDistance;
        Vector3 grabWorld = held.HoldByCenter ? held.Body.worldCenterOfMass : hit.point;

        joint = held.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = holdAnchor;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = held.transform.InverseTransformPoint(grabWorld); // point on the item where the joint will be attached.
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
        joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;

        JointDrive linear = new() { positionSpring = spring, positionDamper = damper, maximumForce = strength };
        joint.xDrive = joint.yDrive = joint.zDrive = linear;

        // Don't let the held item push the player around.
        heldColliders = held.GetComponentsInChildren<Collider>();
        SetPlayerCollision(true);


        // Apparently, this is needed to update the objects position because physics is not updates slower than the frames.
        // The wakeup is to optimize performance, so any object that is sleeping (not moving) is not simulated and saves performance. 
        // IDK how much this is really needed, but it was recommended so i thought it would be good to add. 
        heldPrevInterp = held.Body.interpolation;
        held.Body.interpolation = RigidbodyInterpolation.Interpolate;
        held.Body.WakeUp();

        //grabstate still not used, but just here for consistency.
        SetState(GrabState.Holding);

        Debug.Log($"Grabbed {held.name}"); //debugging

        /*
         * because the grabbing wasn't even working, I commented this out for now. 
        joint.rotationDriveMode = RotationDriveMode.Slerp;
        joint.slerpDrive = new JointDrive { positionSpring = spring * 0.25f, positionDamper = damper * 0.5f, maximumForce = strength };
        */
    }

    private void Release()
    {
        if (held == null) return;

        Debug.Log($"Released {held.name}"); //debugging

        if (joint != null) Destroy(joint);
        SetPlayerCollision(false);
        held.Body.interpolation = heldPrevInterp;  //supposed to restore the interpolation
        held.RemoveHolder();

        joint = null;
        held = null;
        heldColliders = null;
        SetState(GrabState.None); //again, no use as of now
    }

    // to prevent the player from colliding with the item they are grabbing, this is to turn on and off the collisions between
    // player and the item. This is called on release and grab.
    private void SetPlayerCollision(bool ignore)
    {
        if (heldColliders == null) return;
        foreach (var p in playerColliders)
            foreach (var h in heldColliders)
                if (p != null && h != null) Physics.IgnoreCollision(p, h, ignore);
    }

    // Updates the grab state so that it changes something (like a grab icon or movement speed), but isn't used yet.
    private void SetState(GrabState s)
    {
        if (State == s) return;
        State = s;
        StateChanged?.Invoke(s);
    }

}