using UnityEngine;

// Place this on empty GameObjects around the map to define where items can spawn.
// ScatterItems will move one ConsumableItem to each node on Start.
public class ItemSpawnNode : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.08f);
    }
#endif
}
