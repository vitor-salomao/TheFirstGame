using UnityEngine;

public class PlayerCopyMotion : MonoBehaviour
{
    [SerializeField] private Transform targetLimb;
    private ConfigurableJoint joint;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    void Update()
    {
        joint.targetRotation = targetLimb.rotation;
    }
}
