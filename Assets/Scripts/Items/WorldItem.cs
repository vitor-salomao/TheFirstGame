using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour
{
    private const float DefaultWeight = 1f;

    [SerializeField] private ItemData data;

    [Header("Grabbing")]
    [SerializeField] private bool grabbable = true;
    [Tooltip("How easy this is to hold on to. Below 1 = slippery, above 1 = has handles.")]
    [SerializeField, Min(0.1f)] private float gripMultiplier = 1f;
    [Tooltip("Hold by the center of mass instead of where you grabbed it.")]
    [SerializeField] private bool holdByCenter = false;

    public ItemData Data => data;
    public Rigidbody Body { get; private set; }
    public Durability Durability { get; } = new();
    public int Value { get; private set; }
    public int HolderCount { get; private set; }

    public string DisplayName => data != null ? data.Name : name;
    public float Weight => data != null ? data.Weight : DefaultWeight;
    public bool IsGrabbable => grabbable && !Durability.IsBroken;
    public float GripMultiplier => gripMultiplier;
    public bool HoldByCenter => holdByCenter;

    private void Awake()
    {
        Body = GetComponent<Rigidbody>();

        if (data == null) Debug.LogWarning($"{name} has no ItemData assigned.", this);
        ApplyData();
    }

    public void Init(ItemData itemData, int value)
    {
        data = itemData;
        Value = value;
        ApplyData();
    }

    public void AddHolder() => HolderCount++;
    public void RemoveHolder() => HolderCount = Mathf.Max(0, HolderCount - 1);

    private void ApplyData()
    {
        Body.mass = Weight;
        if (data != null) Durability.Reset(data.MaxDurability, data.Indestructible);
    }

    // Keeps the Rigidbody mass correct in the Inspector
    private void OnValidate()
    {
        if (TryGetComponent(out Rigidbody body)) body.mass = Weight;
    }
}
