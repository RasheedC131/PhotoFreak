using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ActionWanderNodes : UtilityAction
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

        if (nodeScript.currentCrowd.Contains(ctx))
        {
            ctx.currentActionState = NPCActionState.IDLE;
            if (agent.isOnNavMesh && !agent.isStopped) agent.isStopped = true;
            return;
        }

        attemptTimer += Time.deltaTime; 

        bool hasArrived = false;
        if (agent.enabled && agent.isOnNavMesh && !agent.pathPending && agent.hasPath && agent.remainingDistance <= gs.wanderMaxDistToDest) hasArrived = true;
        if (Vector3.Distance(ctx.transform.position, ctx.currentDestination) <= gs.wanderMaxDistToDest) hasArrived = true; 
        if (!hasArrived && attemptTimer > 2.0f && Vector3.Distance(ctx.transform.position, ctx.targetNode.position) <= gs.wanderNodeSpreadRadius + 2.0f) hasArrived = true;
        if (!hasArrived && attemptTimer > 15.0f) { AbandonNode(); return; }

        if (hasArrived)
        {
            if (nodeScript.incomingCrowd.Contains(ctx)) nodeScript.incomingCrowd.Remove(ctx);
            if (!nodeScript.currentCrowd.Contains(ctx)) nodeScript.currentCrowd.Add(ctx);
            
            hasReservedSpot = false; 

            if (agent.enabled && agent.isOnNavMesh) 
            {
                agent.ResetPath(); 
                agent.isStopped = true;
            }
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
            ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
            if (nodeScript != null)
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
        List<ZoneNode> validNodes = new List<ZoneNode>();

        foreach (ZoneNode node in allNodes)
        {
            if (ctx.targetNode != null && node.transform == ctx.targetNode) continue;
            if (node.HasOpenSlots()) validNodes.Add(node);
        }

        if (validNodes.Count > 0)
        {
            ZoneNode chosenNode = validNodes[Random.Range(0, validNodes.Count)];
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

    public bool HasOpenNodes()
    {
        if (gs == null) return false;
        foreach (ZoneNode node in FindObjectsOfType<ZoneNode>())
        {
            if (node.HasOpenSlots()) return true;
        }
        return false;
    }

    public override void OnExit()
    {
        if (hasReservedSpot && ctx.targetNode != null)
        {
            ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
            if (nodeScript != null && nodeScript.incomingCrowd.Contains(ctx)) nodeScript.incomingCrowd.Remove(ctx);
            
            
            hasReservedSpot = false;
            ctx.targetNode = null;
            ctx.forceNewPath = false; 
        }
    }
}