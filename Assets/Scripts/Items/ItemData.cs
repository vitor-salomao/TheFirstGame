using UnityEngine;

[CreateAssetMenu(menuName = "Packers/Item")]
public class ItemData : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
    public float Weight;
    public float Durability;
    public ItemType Type;

    public enum ItemType
    {
        Ready,
        Component,
        Trinket
    }
}
