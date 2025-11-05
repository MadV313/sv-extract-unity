using UnityEngine;

public class HitNumbersHook : MonoBehaviour
{
    // placeholder for Damage Numbers Pro (or your chosen hit text system)
    public void ShowDamage(int amount, Vector3 worldPos) {
        Debug.Log($"Hit: {amount} at {worldPos}");
        // integrate your chosen asset here
    }
}
