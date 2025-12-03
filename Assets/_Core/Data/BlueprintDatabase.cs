using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="SV13/Blueprint Database")]
public class BlueprintDatabase : ScriptableObject {
    [SerializeField] List<string> unlocked = new();
    public bool IsUnlocked(string id) => unlocked.Contains(id);
    public void Unlock(string id) { if (!unlocked.Contains(id)) unlocked.Add(id); }
}
