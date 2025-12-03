using UnityEngine;
using System.Collections;

public class CraftingSystem : MonoBehaviour {
    public CraftingDatabase db;
    public PlayerInventory inv;
    public IToolProvider tool;
    public BlueprintDatabase blueprints; // optional; can be null

    public bool CanCraft(string id) {
        var r = db ? db.Find(id) : null;
        if (!r) return false;
        if (!string.IsNullOrEmpty(r.requiredToolId) && (tool?.CurrentToolId != r.requiredToolId)) return false;
        if (!string.IsNullOrEmpty(r.requiresBlueprintId) && blueprints && !blueprints.IsUnlocked(r.requiresBlueprintId)) return false;
        foreach (var s in r.inputs) if (inv.CountOf(s.id) < s.amount) return false;
        return true;
    }

    public void Craft(string id) {
        var r = db ? db.Find(id) : null;
        if (!r || !CanCraft(id)) return;
        StartCoroutine(CoCraft(r));
    }

    IEnumerator CoCraft(CraftingRecipe r) {
        if (r.craftTime > 0f) yield return new WaitForSeconds(r.craftTime);
        foreach (var s in r.inputs) inv.Consume(s.id, s.amount);
        foreach (var s in r.outputs) inv.Add(s.id, s.amount);
    }
}
