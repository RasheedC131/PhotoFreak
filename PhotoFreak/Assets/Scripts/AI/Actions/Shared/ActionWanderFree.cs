using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ActionWanderFree : UtilityAction
{
    private NavMeshAgent agent; 
    private AIContext ctx; 
    private GuestSettings gs; 
    
    public bool hasReservedSpot = false; 
    private float attemptTimer = 0f; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 
        gs = GuestSettings.Instance; 
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null) return; 

        // 1. Pick a new node if we don't have one
        if (ctx.targetNode == null || ctx.forceNewPath)
        {
            ctx.forceNewPath = false; 
            PickNextRandomNode(); 
            if (ctx.targetNode == null) 
            {
                AbandonNode();
                return;
            }
        }

        ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
        if (nodeScript == null)
        {
            AbandonNode();
            return;
        }

        attemptTimer += Time.deltaTime; 

        // 2. Check Arrival
        bool hasArrived = false;
        if (agent.enabled && agent.isOnNavMesh && !agent.pathPending && agent.hasPath && agent.remainingDistance <= gs.wanderMaxDistToDest) hasArrived = true;
        if (Vector3.Distance(ctx.transform.position, ctx.currentDestination) <= gs.wanderMaxDistToDest - 0.5f) hasArrived = true; 
        if (!hasArrived && attemptTimer > 2.0f && Vector3.Distance(ctx.transform.position, ctx.targetNode.position) <= gs.wanderNodeSpreadRadius + 2.0f) hasArrived = true;
        if (!hasArrived && attemptTimer > 15.0f) { AbandonNode(); return; }

        if (hasArrived)
        {
            // Remove from incoming crowd so the slot frees up
            if (nodeScript.incomingCrowd.Contains(ctx)) nodeScript.incomingCrowd.Remove(ctx);
            
            // BYPASSING currentCrowd: We do NOT add the NPC to nodeScript.currentCrowd here.
            // This ensures ConsiderationGroupAvailable scores 0, preventing socialization.
            
            hasReservedSpot = false; 
            
            // Trigger the Idle Spike for a pause (hands off to ActionIdle)
            ctx.forcedIdleEndTime = Time.time + Random.Range(2.0f, 5.0f);
            
            // Clear the target node so they immediately pick a new one when the idle timer expires
            ctx.targetNode = null;
            ctx.forceNewPath = true;
            attemptTimer = 0f;
            
            ctx.currentActionState = NPCActionState.IDLE;
        }
        else
        {
            if (agent.enabled && agent.isOnNavMesh && !agent.pathPending)
            {
                if (agent.isStopped) agent.isStopped = false;
                agent.SetDestination(ctx.currentDestination);
                ctx.currentActionState = NPCActionState.WALK;
            }
        }
    }    

    public void AbandonNode()
    {
        if (ctx.targetNode != null)
        {
            if (ctx.targetNode.TryGetComponent<ZoneNode>(out ZoneNode nodeScript))
            {
                if (nodeScript.incomingCrowd.Contains(ctx)) nodeScript.incomingCrowd.Remove(ctx);   
            }
        }
        
        hasReservedSpot = false;
        ctx.targetNode = null;
        ctx.forceNewPath = false; 
        
        attemptTimer = 0f; 
        ctx.currentActionState = NPCActionState.WALK;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    private void PickNextRandomNode()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();

        List<ZoneNode> preferredNodes = new List<ZoneNode>();  
        List<ZoneNode> fallbackNodes  = new List<ZoneNode>();  

        foreach (ZoneNode node in allNodes)
        {
            if (ctx.targetNode != null && node.transform == ctx.targetNode) continue;
            if (!node.HasOpenSlots()) continue;

            if (IsNearKillRoom(node.transform.position))
                fallbackNodes.Add(node);
            else
                preferredNodes.Add(node);
        }

        List<ZoneNode> pool = preferredNodes.Count > 0 ? preferredNodes : fallbackNodes;

        if (pool.Count > 0)
        {
            ZoneNode chosenNode = pool[Random.Range(0, pool.Count)];
            ctx.targetNode = chosenNode.transform;

            if (!chosenNode.incomingCrowd.Contains(ctx)) chosenNode.incomingCrowd.Add(ctx);
            hasReservedSpot = true;
            attemptTimer = 0f;

            Vector3 randomOffset = Random.insideUnitSphere * gs.wanderNodeSpreadRadius;
            randomOffset.y = 0;
            Vector3 desiredDestination = ctx.targetNode.position + randomOffset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredDestination, out hit, gs.wanderNodeSpreadRadius, NavMesh.AllAreas))
                ctx.currentDestination = hit.position;
            else
                ctx.currentDestination = ctx.targetNode.position;
        }
        else ctx.targetNode = null;
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
        if (ctx.targetNode != null)
        {
            if (ctx.targetNode.TryGetComponent<ZoneNode>(out ZoneNode nodeScript))
            {
                if (nodeScript.incomingCrowd.Contains(ctx)) nodeScript.incomingCrowd.Remove(ctx);
                if (nodeScript.currentCrowd.Contains(ctx))  nodeScript.currentCrowd.Remove(ctx);
            }
        }

        hasReservedSpot  = false;
        ctx.targetNode   = null;
        ctx.forceNewPath = false;
        attemptTimer     = 0f;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}