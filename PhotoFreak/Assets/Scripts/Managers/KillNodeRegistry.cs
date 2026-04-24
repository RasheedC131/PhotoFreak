using UnityEngine;
using System.Collections.Generic;


public class KillNodeRegistry : MonoBehaviour
{
    public static KillNodeRegistry Instance { get; private set; }

    private readonly Dictionary<Transform, AIContext> reservations = new Dictionary<Transform, AIContext>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool TryReserve(Transform node, AIContext reserver)
    {
        if (node == null || reserver == null) return false;

        if (reservations.TryGetValue(node, out AIContext current))
            return current == reserver;   

        reservations[node] = reserver;
        Debug.Log($"[KillNodeRegistry] {reserver.gameObject.name} reserved {node.name}.");
        return true;
    }

    public void Release(Transform node, AIContext reserver)
    {
        if (node == null || reserver == null) return;

        if (reservations.TryGetValue(node, out AIContext current) && current == reserver)
        {
            reservations.Remove(node);
            Debug.Log($"[KillNodeRegistry] {reserver.gameObject.name} released {node.name}.");
        }
    }

    public bool IsReserved(Transform node) => node != null && reservations.ContainsKey(node);

    public bool IsReservedBy(Transform node, AIContext ctx)
        => node != null
           && reservations.TryGetValue(node, out AIContext current)
           && current == ctx;


    public void ClearAll()
    {
        reservations.Clear();
        Debug.Log("[KillNodeRegistry] All reservations cleared.");
    }
}
