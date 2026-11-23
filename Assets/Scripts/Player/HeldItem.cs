using UnityEngine;

// Keeps a hugged item where it was grabbed, relative to the arms, by setting how fast it moves and turns each physics step.
// The item moves with the arms as if it were part of them, so it turns with you instantly and never cuts through you.
// Only pushing it back into place (after a bump, or when it sags) is limited by your strength.
public class HeldItem
{
    private readonly Vector3 offsetFromArms;
    private readonly Quaternion rotationInArms;
    private readonly RigidbodyInterpolation originalInterpolation;
    private readonly CollisionDetectionMode originalCollisionMode;

    private Pose lastArms;
    private Vector3 lastCarryVelocity;

    // Where the item was sent last step, and how fast
    private Vector3 lastTarget;
    private Vector3 lastVelocity;

    public WorldItem Item { get; }

    // How far the item is from its spot in your arms
    public float DistanceFromSpot => (lastTarget - Item.Body.worldCenterOfMass).magnitude;

    // How much speed the item lost last step because something stopped it.
    // Gravity also changes its speed, so that part is expected and doesn't count.
    public float ImpactSpeed => (lastVelocity + Physics.gravity * Time.fixedDeltaTime - Item.Body.linearVelocity).magnitude;

    public HeldItem(WorldItem item, Pose arms)
    {
        Item = item;
        Rigidbody body = item.Body;

        Quaternion toArmsSpace = Quaternion.Inverse(arms.rotation);
        offsetFromArms = toArmsSpace * (body.worldCenterOfMass - arms.position);
        rotationInArms = toArmsSpace * body.rotation;

        lastArms = arms;
        lastCarryVelocity = Vector3.zero;
        lastTarget = body.worldCenterOfMass;
        lastVelocity = body.linearVelocity;

        // Smooth to look at, and doesn't pass through walls when you turn fast
        originalInterpolation = body.interpolation;
        originalCollisionMode = body.collisionDetectionMode;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void Follow(Pose arms, float strength)
    {
        Rigidbody body = Item.Body;

        Vector3 targetPosition = arms.position + arms.rotation * offsetFromArms;
        Quaternion targetRotation = arms.rotation * rotationInArms;

        // a = F / m
        MoveToward(body, arms, targetPosition, strength / body.mass);
        TurnToward(body, targetRotation);

        lastArms = arms;
    }

    public void Drop()
    {
        Item.Body.interpolation = originalInterpolation;
        Item.Body.collisionDetectionMode = originalCollisionMode;
    }

    private void MoveToward(Rigidbody body, Pose arms, Vector3 target, float maxAcceleration)
    {
        Vector3 position = body.worldCenterOfMass;

        // 1. Carried along: where the item would be if it moved exactly like the arms this step
        Vector3 carried = CarryWithArms(position, arms);
        Vector3 carryVelocity = (carried - position) / Time.fixedDeltaTime;

        // 2. Correction: from there back to its spot, if it was bumped or is sagging
        Vector3 correction = GetCorrection(body, target - carried, maxAcceleration);

        body.linearVelocity = carryVelocity + correction;
        lastCarryVelocity = carryVelocity;
        lastTarget = target;
        lastVelocity = body.linearVelocity;
    }

    // Moves a point the same way the arms moved since the last step: shifted with them and swung around them
    private Vector3 CarryWithArms(Vector3 point, Pose arms)
    {
        Quaternion turn = arms.rotation * Quaternion.Inverse(lastArms.rotation);
        return arms.position + turn * (point - lastArms.position);
    }

    // Limited by your strength, so heavy items are harder to push back into place
    private Vector3 GetCorrection(Rigidbody body, Vector3 offTarget, float maxAcceleration)
    {
        float distance = offTarget.magnitude;

        // Fast enough to fix it in one step, but never faster than it could still brake in time (v = sqrt(2 * a * d))
        float speed = Mathf.Min(distance / Time.fixedDeltaTime, Mathf.Sqrt(2f * maxAcceleration * distance));
        Vector3 wanted = distance > 0f ? offTarget / distance * speed : Vector3.zero;

        // The part of the item's current speed that is not from being carried, like falling or being bumped
        Vector3 current = body.linearVelocity - lastCarryVelocity;
        float maxChange = maxAcceleration * Time.fixedDeltaTime;

        return current + Vector3.ClampMagnitude(wanted - current, maxChange);
    }

    // Rotation matches the arms exactly, because arms clamped around an item don't let it spin on its own
    private void TurnToward(Rigidbody body, Quaternion target)
    {
        Quaternion difference = target * Quaternion.Inverse(body.rotation);
        difference.ToAngleAxis(out float angle, out Vector3 axis);

        // Turn the short way around
        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) < 0.01f) body.angularVelocity = Vector3.zero;
        else body.angularVelocity = axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}
