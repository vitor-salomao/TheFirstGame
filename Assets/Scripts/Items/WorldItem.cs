using System;
using UnityEngine;

/// All grabbable items in the game should have this component. It handles durability, weight, and other properties that affect how the player can interact with it. 
/// Other things like walls, doors, and terrain do not need this component, since they are not grabbable.

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string displayName = "Item";

    [Header("Physical")]
    [Tooltip("Mass in kg. This is written to the Rigidbody, so set weight HERE, not on the Rigidbody.")]
    [SerializeField, Min(0.01f)] private float weight = 1f;

    [Header("Durability")]
    [SerializeField, Min(1f)] private float maxDurability = 100f;
    [SerializeField] private bool indestructible = false;

    [Header("Grabbing")]
    [SerializeField] private bool grabbable = true;
    [Tooltip("How easy this is to hold on to. <1 = slippery/awkward, >1 = has handles.")]
    [SerializeField, Min(0.1f)] private float gripMultiplier = 1f;
    [Tooltip("Hold by the center of mass instead of where you grabbed it (small items).")]
    [SerializeField] private bool holdByCenter = false;

    
    public string DisplayName => displayName;
    public float Weight => weight;
    public Rigidbody Body { get; private set; }

    public float Durability { get; private set; }
    public float MaxDurability => maxDurability;
    public bool IsBroken => !indestructible && Durability <= 0f;

    public bool IsGrabbable => grabbable && !IsBroken;
    public float GripMultiplier => gripMultiplier;
    public bool HoldByCenter => holdByCenter;

    /// How many players are holding this right now (group carrying). Not shown in the inspector.
    /// Recommended by Claude. 
    public int HolderCount { get; private set; }

    // Grabbing (called by PlayerGrab in player scripts) 
    public void AddHolder() => HolderCount++;
    public void RemoveHolder() => HolderCount = Mathf.Max(0, HolderCount - 1);

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody>();
        Body.mass = weight;
        Durability = maxDurability;
    }

    // Runs in the editor whenever you change a value in the inspector:
    // keeps the Rigidbody's mass in sync with weight so they can never disagree.
    protected virtual void OnValidate()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.mass = weight;
    }


    // Breaking 

    /// Fired once when durability hits zero.
    public event Action<WorldItem> Broken;

    /// Override in subclasses for custom break behaviour (shatter, spawn pieces, etc.).
    protected virtual void OnBroken() => Broken?.Invoke(this);

    ///Durability
    public void Damage(float amount)
    {
        if (indestructible || IsBroken || amount <= 0f) return;
        Durability = Mathf.Max(0f, Durability - amount);
        if (IsBroken) OnBroken();
    }

    /* 
     * Claude recommended a repair function, but I don't think we need it. If we do, uncomment this code:
    public void Repair(float amount)
    {
        Durability = Mathf.Min(maxDurability, Durability + Mathf.Max(0f, amount));
    }
    */
}