using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 

public class ActionWanderNodes : UtilityAction
{
    private NavMeshAgent agent; 
    private AIContext ctx; 
    private GuestSettings gs; 

    private float movementTime = 0f; 

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 
        gs = GuestSettings.Instance; 
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null) return; 

        if (ctx.targetHub == null) ctx.isOccupied = false;

        if (Time.time < movementTime)
        {
            agent.isStopped = true;
            return; 
        }
        
        agent.isStopped = false; 

        bool hasArrived = !agent.pathPending && agent.hasPath && (agent.remainingDistance <= gs.wanderMaxDistToDest);

        if (hasArrived)
        {
            float waitDuration = Random.Range(gs.wanderMinWaitAtNode, gs.wanderMaxWaitAtNode);
            movementTime = Time.time + waitDuration;
            agent.isStopped = true;
            agent.ResetPath(); 
            PickNextNodeGlobally();
            
            return; 
        }

        if (!agent.hasPath || ctx.forceNewPath)
        {
            ctx.forceNewPath = false; 

            if (ctx.targetNode == null) PickNextNodeGlobally();

            if (ctx.targetNode != null)
            {
                Vector3 randomOffset = Random.insideUnitSphere * gs.wanderNodeSpreadRadius; 
                randomOffset.y = 0;
                Vector3 pathingNoise = Random.insideUnitSphere * 1.5f; 
                pathingNoise.y = 0;
                
                ctx.currentDestination = ctx.targetNode.position + randomOffset + pathingNoise;
            }
            else
            {
                ctx.currentDestination = GetRandomFreeWanderPoint(transform.position, 10f);
            }

            agent.SetDestination(ctx.currentDestination);        
        }
    }

    private void PickNextNodeGlobally()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        List<ZoneNode> validNodes = new List<ZoneNode>();

        foreach (ZoneNode node in allNodes)
        {
            if (ctx.targetNode != null && node.transform == ctx.targetNode) continue;

            if (node.GetCurrentCrowd() < node.activeCapacity)
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count > 0)
        {
            int randomIdx = Random.Range(0, validNodes.Count);
            ctx.targetNode = validNodes[randomIdx].transform;
        }
        else
        {
            ctx.targetNode = null;
        }
    }

    private Vector3 GetRandomFreeWanderPoint(Vector3 origin, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += origin;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas)) return hit.position;
        
        return origin; 
    }

    public bool IsWaiting()
    {
        return Time.time < movementTime;
    }
}