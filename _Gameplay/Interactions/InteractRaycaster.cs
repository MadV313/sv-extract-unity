using UnityEngine;

public class InteractRaycaster : MonoBehaviour
{
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask = ~0;

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var cam = Camera.main;
            if (!cam) return;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, distance, mask)) {
                // TODO: Invoke IInteractable on hit
                Debug.Log($"Interacted with {hit.collider.name}");
            }
        }
    }
}
