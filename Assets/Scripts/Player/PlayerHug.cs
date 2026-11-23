using UnityEngine;

// Right click opens the arms, left click closes them. Closing both arms on the same item picks it up.
public class PlayerHug : MonoBehaviour
{
    [Header("Arms")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Rigidbody armsAnchor;
    [SerializeField] private HugArm leftArm;
    [SerializeField] private HugArm rightArm;

    [Header("Arm Angles")]
    [SerializeField] private float openAngle = 70f;
    [SerializeField] private float relaxedAngle = 15f;
    [SerializeField] private float closedAngle = -30f;
    [Tooltip("How far from straight sideways a surface can face and still count as a side you can squeeze.")]
    [SerializeField, Range(0f, 90f)] private float maxGripAngle = 50f;

    [Header("Holding")]
    [Tooltip("Max force (N) your arms can apply. About 10 N per kg, so 400 = 40 kg.")]
    [SerializeField] private float strength = 400f;

    [Header("Dropping")]
    [Tooltip("If a held item hits something at this speed (m/s) or faster, you drop it.")]
    [SerializeField] private float dropImpactSpeed = 4f;
    [Tooltip("If a held item gets pushed this far (m) from its spot in your arms, you drop it.")]
    [SerializeField] private float dropDistance = 0.4f;

    private PlayerActions.OnFootActions onFoot;
    private HeldItem held;

    public WorldItem Held => held != null ? held.Item : null;

    private void Awake()
    {
        onFoot = new PlayerActions().OnFoot;
    }

    private void OnEnable() => onFoot.Enable();

    private void OnDisable()
    {
        Release();
        onFoot.Disable();
    }

    private void FixedUpdate()
    {
        Pose arms = GetArmsPose();
        armsAnchor.MovePosition(arms.position);
        armsAnchor.MoveRotation(arms.rotation);

        if (held == null) MoveArms();
        else if (ShouldLetGo()) Release();
        else held.Follow(arms, strength);
    }

    private bool ShouldLetGo()
    {
        if (!onFoot.Grab.IsPressed()) return true;

        if (held.ImpactSpeed >= dropImpactSpeed)
        {
            Debug.Log($"{held.Item.name} hit something too hard ({held.ImpactSpeed:F1} m/s)");
            return true;
        }

        if (held.DistanceFromSpot >= dropDistance)
        {
            Debug.Log($"{held.Item.name} got pushed out of your arms ({held.DistanceFromSpot:F2} m)");
            return true;
        }

        return false;
    }

    // Where the arms are attached and which way they face. This is a Pose, not a Transform,
    // because the player object is stretched and that would distort positions when you look up or down.
    private Pose GetArmsPose()
    {
        return new Pose(cameraPivot.position, cameraPivot.rotation);
    }

    private void MoveArms()
    {
        if (onFoot.OpenArms.IsPressed())
        {
            SetArmAngles(openAngle);
        }
        else if (onFoot.Grab.IsPressed())
        {
            SetArmAngles(closedAngle);
            TryHug();
        }
        else
        {
            SetArmAngles(relaxedAngle);
        }
    }

    private void TryHug()
    {
        WorldItem item = leftArm.TouchedItem;
        if (item == null || item != rightArm.TouchedItem || !item.IsGrabbable) return;
        if (!IsSqueezingFromBothSides()) return;

        Hug(item);
    }

    // Both arms must press on opposite sides of the item, not on its top or front
    private bool IsSqueezingFromBothSides()
    {
        bool bothOnSides = PressesOnSide(leftArm) && PressesOnSide(rightArm);
        bool oppositeSides = Vector3.Dot(leftArm.TouchNormal, rightArm.TouchNormal) < 0f;
        return bothOnSides && oppositeSides;
    }

    private bool PressesOnSide(HugArm arm)
    {
        float angle = Vector3.Angle(arm.TouchNormal, armsAnchor.transform.right);
        float angleFromSideways = Mathf.Min(angle, 180f - angle);
        return angleFromSideways <= maxGripAngle;
    }

    private void Hug(WorldItem item)
    {
        held = new HeldItem(item, GetArmsPose());
        item.AddHolder();

        // Keep the arms where they touched the item instead of crushing it
        leftArm.SetTargetAngle(leftArm.Angle);
        rightArm.SetTargetAngle(rightArm.Angle);
        IgnoreArms(item, true);

        Debug.Log($"Hugged {item.name}");
    }

    private void Release()
    {
        if (held == null) return;

        WorldItem item = held.Item;
        Debug.Log($"Released {item.name}");

        held.Drop();
        IgnoreArms(item, false);
        item.RemoveHolder();

        held = null;
    }

    private void SetArmAngles(float angle)
    {
        leftArm.SetTargetAngle(angle);
        rightArm.SetTargetAngle(angle);
    }

    // The held item would fight the arms that are wrapped around it
    private void IgnoreArms(WorldItem item, bool ignore)
    {
        foreach (Collider itemCollider in item.GetComponentsInChildren<Collider>())
        {
            foreach (Collider armCollider in leftArm.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(armCollider, itemCollider, ignore);
            foreach (Collider armCollider in rightArm.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(armCollider, itemCollider, ignore);
        }
    }
}
