using UnityEngine;

// One physical arm that swings left and right on a hinge at the shoulder
[RequireComponent(typeof(Rigidbody), typeof(HingeJoint))]
public class HugArm : MonoBehaviour
{
    private HingeJoint hinge;
    private WorldItem touchedItem;
    private float lastTouchTime;

    public Rigidbody Body { get; private set; }
    public float Angle => hinge.angle;

    // The direction the touched surface faces
    public Vector3 TouchNormal { get; private set; }

    // Only counts if the arm touched the item during the last physics step
    public WorldItem TouchedItem => Time.fixedTime - lastTouchTime <= Time.fixedDeltaTime ? touchedItem : null;

    private void Awake()
    {
        Body = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
    }

    // 0 = straight ahead, positive = outward, negative = inward
    public void SetTargetAngle(float angle)
    {
        JointSpring spring = hinge.spring;
        spring.targetPosition = angle;
        hinge.spring = spring;
    }

    private void OnCollisionStay(Collision collision)
    {
        WorldItem item = collision.collider.GetComponentInParent<WorldItem>();
        if (item == null) return;

        touchedItem = item;
        TouchNormal = GetAverageNormal(collision);
        lastTouchTime = Time.fixedTime;
    }

    private Vector3 GetAverageNormal(Collision collision)
    {
        Vector3 total = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++) total += collision.GetContact(i).normal;
        return total.normalized;
    }
}
