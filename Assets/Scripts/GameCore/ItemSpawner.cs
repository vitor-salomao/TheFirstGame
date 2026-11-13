using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Tooltip("Parent of all SpawnSlots on the current map. Read in hierarchy order so seeds stay reproducible")]
    [SerializeField] private Transform slotRoot;
    [Tooltip("Spawned items are parented here to keep hierarchy organized")]
    [SerializeField] private Transform itemContainer;

    private readonly List<WorldItem> spawned = new List<WorldItem>
    public IreadOnlyList<WorldItem> Spawned => spawned;

    // This basically creates an array of all of the spawned slots for Small, Medium, and Large items. The order is important for the seed system to work properly.
    public int[] GetCapacity()
    {
        var capacity = new int[3];
        foreach (var slot in GetSlots()) capacity[(int)slot.Size]++;
        return capacity;
    }

    public void Spawn(ShipmentPlan plan, System.Random rng)
    {
        Clear();
        
        // Creates a list of slots for Small, Medium, and Large items. 
        var bySize = new List<SpawnSlot>[3];
        for (int s = 0; s <3; s++) bySize[s] = new List<SpawnSlot>();
        // Goes through every slot on the map and adds each one to the list for its size. Then randomizes
        foreach (var slot in GetSlots()) bySize[(int)slot.Size].Add(slot);
        foreach (var list in bySize) Shuffle(list, rng);

        foreach (var entry in plan.Entries)
        {
            var list = bySize[(int)entry.SlotSize];
            if (list.Count == 0)
            {
                Debug.LogError($"No Free {entry.SlotSize} slot for {entry.Item.Name}", this);
                continue;
            }

            var slot = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            //Instantiates the new item
            var item = Instantiate(entry.Item.Prefab, slotRoot.transform.position, slotRoot.transform.rotation, itemContainer);
            item.Init(entry.Item, entry.Value);
            SitOnSurface(item.transform, slotRoot.transform.position.y);
            spawned.Add(item);
        }
    }
    
    // This is called at the start of each day by spawn to remove all the items from the previous day. 
    public void Clear()
    {
        foreach (var item in spawned)
            if (item != null) Destroy(item.gameObject);
        spawned.Clear();
    }

    private SpawnSlot[] GetSlots() =>
        slotRoot != null ? slotRoot.GetComponentsInChildren<SpawnSlot>() : new SpawnSlot[0];

    //prevents item from clashing with the ground by lifting it so it sits on the surface of the slot
    private static void SitOnSurface(Transform item, float surfaceY)
    {
        var renderers = item.GetComponentsInChildren<renderers>();
        if (renderes.Length == 0) return;

        var bounds = rederers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        item.position += Vector3.up * (surafceY - bounds.min.y + 0.01f);
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i >0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}
