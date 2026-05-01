using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ActionWanderFree : UtilityAction
{
    private NavMeshAgent  agent;
    private AIContext     ctx;
    private GuestSettings gs;

    // Private roaming state — deliberately NOT stored on ctx so that
    // ConsiderationWanderNodes never sees a targetNode and mis-scores high.
    private Transform _roamNode        = null;
    private Vector3   _roamDestination = Vector3.positiveInfinity;
    private Vector3   _lastSetDest     = Vector3.positiveInfinity;
    private float     _attemptTimer    = 0f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx   = GetComponentInParent<AIContext>();
        gs    = GuestSettings.Instance;
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null) return;

        // Pick a roam waypoint if we don't have one
        if (_roamNode == null)
        {
            PickNextRoamNode();
            if (_roamNode == null)
            {
                ctx.currentActionState = NPCActionState.IDLE;
                return;
            }
        }

        _attemptTimer += Time.deltaTime;

        // Arrival checks
        bool hasArrived = false;
        if (agent.enabled && agent.isOnNavMesh && !agent.pathPending && agent.hasPath
            && agent.remainingDistance <= gs.wanderMaxDistToDest)
            hasArrived = true;
        if (Vector3.Distance(ctx.transform.position, _roamDestination) <= gs.wanderMaxDistToDest - 0.5f)
            hasArrived = true;
        if (!hasArrived && _attemptTimer > 2.0f
            && Vector3.Distance(ctx.transform.position, _roamNode.position) <= gs.wanderNodeSpreadRadius + 2.0f)
            hasArrived = true;

        // Timeout: give up and pick a different waypoint rather than standing still
        if (!hasArrived && _attemptTimer > 15.0f)
        {
            _roamNode    = null;
            _lastSetDest = Vector3.positiveInfinity;
            _attemptTimer = 0f;
            return;
        }

        if (hasArrived)
        {
            _attemptTimer = 0f;
            _lastSetDest  = Vector3.positiveInfinity;
            ctx.currentActionState = NPCActionState.IDLE;

            // Brief pause at the waypoint, then move on
            ctx.forcedIdleEndTime = Time.time + Random.Range(1.5f, 4.0f);
            _roamNode = null; // cleared so the next tick picks a new waypoint

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
        else
        {
            if (agent.enabled && agent.isOnNavMesh && !agent.pathPending)
            {
                if (agent.isStopped) agent.isStopped = false;
                ctx.currentActionState = NPCActionState.WALK;

                bool destChanged = Vector3.SqrMagnitude(_roamDestination - _lastSetDest) > 0.01f;
                if (destChanged || !agent.hasPath)
                {
                    agent.SetDestination(_roamDestination);
                    _lastSetDest = _roamDestination;
                }
            }
        }
    }

    /// <summary>
    /// Picks any zone node as a free-roaming waypoint.
    /// Does NOT touch ctx.targetNode — keeping that field null prevents
    /// ConsiderationWanderNodes from reading it as a WanderNodes commitment
    /// and causing the brain to flip back and forth.
    /// </summary>
    private void PickNextRoamNode()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();

        List<ZoneNode> preferred = new List<ZoneNode>();
        List<ZoneNode> fallback  = new List<ZoneNode>();

        foreach (ZoneNode node in allNodes)
        {
            if (_roamNode != null && node.transform == _roamNode) continue; // skip current

            if (IsNearKillRoom(node.transform.position))
                fallback.Add(node);
            else
                preferred.Add(node);
        }

        List<ZoneNode> pool = preferred.Count > 0 ? preferred : fallback;
        if (pool.Count == 0) { _roamNode = null; return; }

        ZoneNode chosen = pool[Random.Range(0, pool.Count)];
        _roamNode     = chosen.transform;
        _attemptTimer = 0f;

        Vector3 offset = Random.insideUnitSphere * gs.wanderNodeSpreadRadius;
        offset.y = 0f;
        Vector3 desired = _roamNode.position + offset;

        NavMeshHit hit;
        _roamDestination = NavMesh.SamplePosition(desired, out hit, gs.wanderNodeSpreadRadius, NavMesh.AllAreas)
            ? hit.position
            : _roamNode.position;
    }

    private bool IsNearKillRoom(Vector3 pos)
    {
        if (gs == null) return false;
        foreach (Transform killNode in ActionIsolate.AllKillNodes)
        {
            if (killNode != null && Vector3.Distance(pos, killNode.position) < gs.killRoomAvoidRadius)
                return true;
        }
        return false;
    }

    public override void OnExit()
    {
        // Clear local state only — ctx.targetNode is untouched so WanderNodes
        // can pick up cleanly when it takes over
        _roamNode     = null;
        _lastSetDest  = Vector3.positiveInfinity;
        _attemptTimer = 0f;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}
