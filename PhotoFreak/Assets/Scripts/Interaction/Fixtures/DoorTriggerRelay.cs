using UnityEngine;

// Place this on the child door object that has the Box Collider (Is Trigger).
// It forwards trigger events up to the Door component on the pivot parent,
// since OnTriggerEnter/Exit only fire on the same GameObject as the collider.
[DisallowMultipleComponent]
public class DoorTriggerRelay : MonoBehaviour
{
    private Door door;

    private void Awake()
    {
        door = GetComponentInParent<Door>();

        if (door == null)
            Debug.LogWarning($"{gameObject.name}: DoorTriggerRelay could not find a Door component in its parent hierarchy.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (door != null) door.NotifyTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (door != null) door.NotifyTriggerExit(other);
    }
}
