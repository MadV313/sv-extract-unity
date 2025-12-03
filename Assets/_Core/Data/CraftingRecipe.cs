using UnityEngine;

[CreateAssetMenu(menuName="SV13/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject {
    public string recipeId;
    [System.Serializable] public struct Stack { public string id; public int amount; }
    public Stack[] inputs;
    public Stack[] outputs;
    public string requiredToolId;         // optional (e.g., tool_saw)
    public string requiresBlueprintId;    // optional
    public float craftTime = 0f;          // optional delay
}
