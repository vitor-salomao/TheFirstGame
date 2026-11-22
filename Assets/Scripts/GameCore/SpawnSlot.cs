using UnityEngine;

public class SpawnSlot : MonoBehaviour
{
    [Tooltip("The biggest item that fits here.")]
    [SerializeField] private ItemSize size = ItemSize.Small;

    public ItemSize Size => size;

    private void OnDrawGizmos()
    {
        float width = 0.3f;
        Gizmos.color = Color.green;

        if (size == ItemSize.Medium)
        {
            width = 0.6f;
            Gizmos.color = Color.yellow;
        }
        else if (size == ItemSize.Large)
        {
            width = 1.2f;
            Gizmos.color = Color.red;
        }

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.up * width / 2f, Vector3.one * width);
    }
}
