using UnityEngine;

// Distributes ConsumableItems already in the scene across ItemSpawnNodes at runtime
public class ScatterItems : MonoBehaviour
{
    [SerializeField] private float spawnHeightOffset = 0.15f;
    [SerializeField] private bool shuffleItems = true;

    private void Start()
    {
        ItemSpawnNode[] nodes = FindObjectsOfType<ItemSpawnNode>();
        ConsumableItem[] items = FindObjectsOfType<ConsumableItem>(true);

        if (nodes.Length == 0)
        {
            Debug.LogWarning("ScatterItems: No ItemSpawnNodes found in the scene.");
            return;
        }

        if (items.Length == 0)
        {
            Debug.LogWarning("ScatterItems: No ConsumableItems found in the scene.");
            return;
        }

        if (shuffleItems) ShuffleArray(items);

        int assignCount = Mathf.Min(nodes.Length, items.Length);

        // Assign one item to each node
        for (int i = 0; i < assignCount; i++)
        {
            PlaceItem(items[i], nodes[i]);
        }

        // Deactivate any items that didn't receive a node
        for (int i = assignCount; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(false);
        }

        if (items.Length < nodes.Length)
            Debug.Log($"ScatterItems: {nodes.Length - items.Length} node(s) left empty (not enough items).");

        if (items.Length > nodes.Length)
            Debug.Log($"ScatterItems: {items.Length - nodes.Length} item(s) deactivated (not enough nodes).");
    }

    private void PlaceItem(ConsumableItem item, ItemSpawnNode node)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();

        // Briefly make kinematic so the item doesn't drift during placement
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity   = Vector3.zero;
            rb.angularVelocity  = Vector3.zero;
        }

        // Position above the node so gravity drops it onto the surface
        item.transform.SetParent(null);
        item.transform.position = node.transform.position + Vector3.up * spawnHeightOffset;
        item.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        item.gameObject.SetActive(true);

        // Re-enable physics so the item settles naturally
        if (rb != null) rb.isKinematic = false;
    }

    private void ShuffleArray(ConsumableItem[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex   = Random.Range(i, array.Length);
            ConsumableItem temp = array[i];
            array[i]          = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
