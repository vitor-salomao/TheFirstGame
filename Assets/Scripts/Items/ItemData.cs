using UnityEngine;

[CreateAssetMenu(menuName = "Packers/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string Name = "Item";
    public WorldItem Prefab;
    public ItemType Type;
    public ItemSize Size;

    [Header("Physical")]
    [Tooltip("Mass in kg.")]
    [Min(0.01f)] public float Weight = 1f;

    [Header("Durability")]
    [Min(1f)] public float MaxDurability = 100f;
    public bool Indestructible = false;

    [Header("Price")]
    [Min(0)] public int MinValue = 5;
    [Min(0)] public int MaxValue = 10;

    [Header("Spawning")]
    [Tooltip("Relative chance to be picked. 2 = twice as likely as 1.")]
    [Min(0f)] public float SpawnWeight = 1f;
    [Tooltip("First day this item can appear.")]
    [Min(1)] public int MinDay = 1;

    public int RollValue(System.Random random)
    {
        return random.Next(MinValue, MaxValue + 1);
    }

    public bool CanSpawn(int day, int maxValue)
    {
        if (Prefab == null) return false;
        if (Type != ItemType.Ready) return false;
        if (SpawnWeight <= 0f) return false;
        if (day < MinDay) return false;

        return MinValue <= maxValue;
    }

    private void OnValidate()
    {
        if (MaxValue < MinValue) MaxValue = MinValue;
    }
}
