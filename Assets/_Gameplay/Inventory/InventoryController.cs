using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [System.Serializable]
    public class Stack { public ItemDef Item; public int Count; }

    [SerializeField] private int slots = 8;
    [SerializeField] private List<Stack> contents = new();

    public bool TryAdd(ItemDef item, int count=1) {
        // simple placeholder logic
        var slot = contents.Find(s => s.Item == item && s.Count < item.MaxStack);
        if (slot != null) { slot.Count = Mathf.Min(item.MaxStack, slot.Count + count); return true; }
        if (contents.Count >= slots) return false;
        contents.Add(new Stack { Item = item, Count = Mathf.Clamp(count, 1, item.MaxStack) });
        return true;
    }
}
