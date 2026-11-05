using System;
using UnityEngine;

/// All grabbable items in the game should have this component. It handles durability, weight, and other properties that affect how the player can interact with it. 
/// Other things like walls, doors, and terrain do not need this component, since they are not grabbable.

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour
{

    [Header("Blueprint")]
    [Tooltip("Assign on the prefab. Weight and durability come from here.")]
    [SerializeField] private ItemData data;

    [Header("Grabbing")]
    [SerializeField] private bool grabbable = true;
    [Tooltip("How easy this is to hold on to. <1 = slippery/awkward, >1 = has handles.")]
    [SerializeField, Min(0.1f)] private float gripMultiplier = 1f;
    [Tooltip("Hold by the center of mass instead of where you grabbed it (small items).")]
    [SerializeField] private bool holdByCenter = false;


    public ItemData Data => data;
    public string DisplayName => data != null ? data.Name : name;
    public float Weight => data != null ? data.Weight : 1f;
    public Rigidbody Body { get; private set; }

    public float Durability { get; private set; }
    public float MaxDurability => data != null ? data.MaxDurability : 100f;
    public bool Indestructible => data != null && data.Indestructible;
    public bool IsBroken => !Indestructible && Durability <= 0f;

    public bool IsGrabbable => grabbable && !IsBroken;
    public float GripMultiplier => gripMultiplier;
    public bool HoldByCenter => holdByCenter;

    /// Rolled by the spawner. 0 for items placed by hand in the scene.
    public int Value { get; private set; }

    /// How many players are holding this right now (group carrying).
    public int HolderCount { get; private set; }
    public void AddHolder() => HolderCount++;
    public void RemoveHolder() => HolderCount = Mathf.Max(0, HolderCount - 1);

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody>();
        if (data == null)
            Debug.LogWarning($"{name} has no ItemData assigned.", this);
        ApplyBlueprint();
    }

    // Called by the spawner right after Instantiate. Pass a different ItemData for variants.
    public void Init(ItemData itemData, int value)
    {
        data = itemData;
        Value = value;
        ApplyBlueprint();
    }

    private void ApplyBlueprint()
    {
        Body.mass = Weight;
        Durability = MaxDurability;
    }

    // Runs in the editor whenever you change a value in the inspector:
    // keeps the Rigidbody's mass in sync with weight so they can never disagree.
    protected virtual void OnValidate()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.mass = data.Weight;
    }


    // Breaking 

    /// Fired once when durability hits zero.
    public event Action<WorldItem> Broken;

    /// Override in subclasses for custom break behaviour (shatter, spawn pieces, etc.).
    protected virtual void OnBroken() => Broken?.Invoke(this);

    ///Durability
    public void Damage(float amount)
    {
        if (Indestructible || IsBroken || amount <= 0f) return;
        Durability = Mathf.Max(0f, Durability - amount);
        if (IsBroken) OnBroken();
    }

}