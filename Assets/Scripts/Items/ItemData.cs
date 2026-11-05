using UnityEngine;

[CreateAssetMenu(menuName = "Packers/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string Name = "Item";
    [Tooltip("Must have a WorldItem component, with this asset assigned to it.")]
    public WorldItem Prefab;
    public ItemType Type;
    public ItemSize Size;

    [Header("Physical")]
    [Tooltip("Mass in kg. Written to the Rigidbody on spawn.")]
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

   // Generator calls this once per spawned item to get that item's value.
    public int RollValue(System.Random rng) => rng.Next(MinValue, MaxValue + 1);

    // Changing item in inspector should not allow MinValue to be greater than MaxValue. This is enforced in OnValidate.
    private void OnValidate()
    {
        if (MaxValue < MinValue) MaxValue = MinValue;
    }

    public enum ItemType { 
        Ready, 
        Component, 
        Trinket 
    }
    
    public enum ItemSize { 
        Small, 
        Medium, 
        Large 
    }
}