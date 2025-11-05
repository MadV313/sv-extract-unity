using UnityEngine;

[CreateAssetMenu(menuName = "SV13/Item Definition", fileName = "ItemDef_")]
public class ItemDef : ScriptableObject
{
    public string Id;            // e.g. "bandage"
    public string DisplayName;   // "Bandage"
    public Sprite Icon;
    public int MaxStack = 5;
}
