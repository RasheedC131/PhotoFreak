using UnityEngine;
using System.Collections.Generic;

// Static registry — never null, no scene setup required.
// RuntimeInitializeOnLoadMethod clears stale state each play session so
// editor runs without domain reload don't carry over old reservations.
public static class KillNodeRegistry
{
    private static readonly Dictionary<Transform, AIContext> _reservations
        = new Dictionary<Transform, AIContext>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _reservations.Clear();
        Debug.Log("[KillNodeRegistry] Cleared for new session.");
    }

    // Returns true if the reservation was taken (or was already held by this reserver).
    public static bool TryReserve(Transform node, AIContext reserver)
    {
        if (node == null || reserver == null) return false;

        if (_reservations.TryGetValue(node, out AIContext current))
            return current == reserver;   // already held by this NPC — resume OK

        _reservations[node] = reserver;
        Debug.Log($"[KillNodeRegistry] {reserver.gameObject.name} reserved {node.name}.");
        return true;
    }

    public static void Release(Transform node, AIContext reserver)
    {
        if (node == null || reserver == null) return;

        if (_reservations.TryGetValue(node, out AIContext current) && current == reserver)
        {
            _reservations.Remove(node);
            Debug.Log($"[KillNodeRegistry] {reserver.gameObject.name} released {node.name}.");
        }
    }

    public static bool IsReserved(Transform node)
        => node != null && _reservations.ContainsKey(node);

    public static bool IsReservedBy(Transform node, AIContext ctx)
        => node != null
           && _reservations.TryGetValue(node, out AIContext current)
           && current == ctx;

    // True if this NPC already holds a reservation, OR at least one kill node
    // is free. False when every node is taken by a different NPC — the caller
    // should score ActionIsolate as 0 and let the NPC keep wandering.
    public static bool HasAvailableNode(AIContext ctx)
    {
        foreach (Transform node in ActionIsolate.AllKillNodes)
        {
            if (node == null) continue;
            if (IsReservedBy(node, ctx)) return true;   // this NPC already has a spot
        }

        foreach (Transform node in ActionIsolate.AllKillNodes)
        {
            if (node == null) continue;
            if (!IsReserved(node)) return true;          // a free slot exists
        }

        return false;
    }

    public static void ClearAll()
    {
        _reservations.Clear();
        Debug.Log("[KillNodeRegistry] All reservations cleared.");
    }
}
