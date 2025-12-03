using UnityEngine;

[CreateAssetMenu(menuName="SV13/Crafting Database")]
public class CraftingDatabase : ScriptableObject {
    public CraftingRecipe[] recipes;
    public CraftingRecipe Find(string id) {
        foreach (var r in recipes) if (r && r.recipeId == id) return r;
        return null;
    }
}
