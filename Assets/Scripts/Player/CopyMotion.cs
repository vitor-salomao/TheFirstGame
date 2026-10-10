using UnityEngine;

public class PlayerCopyMotion : MonoBehaviour
{
    [SerializeField] private Transform targetLimb;
    [SerializeField] private bool mirror;
    private ConfigurableJoint joint;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    void Update()
    {
        joint.targetRotation = mirror ? Quaternion.Inverse(targetLimb.rotation) : targetLimb.rotation;
    }
}
