using UnityEngine;

/// Marks a place where an item can spawn. Place it on the surface (shelf, floor, pallet).
/// An item fits if its size is <= the slot's size.
public class SpawnSlot : MonoBehaviour
{
    [SerializeField] private ItemData.ItemSize size = ItemData.ItemSize.Small;
    public ItemData.ItemSize Size => size;

    private static readonly Vector3[] GizmoSizes =
    {
        new Vector3(0.3f, 0.3f, 0.3f),
        new Vector3(0.6f, 0.6f, 0.6f),
        new Vector3(1.2f, 1.2f, 1.2f)
    };
    private static readonly Color[] GizmoColors = { Color.green, Color.yellow, Color.red };

    private void OnDrawGizmos()
    {
        var box = GizmoSizes[(int)size];
        Gizmos.color = GizmoColors[(int)size];
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.up * box.y / 2f, box);
    }
}