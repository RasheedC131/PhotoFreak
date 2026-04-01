using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 

public class ActionWanderNodes : UtilityAction
{
    private NavMeshAgent agent; 
    private NavMeshObstacle obstacle;
    private AIContext ctx; 
    private GuestSettings gs; 

    private float movementTime = 0f; 

void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        obstacle = GetComponentInParent<NavMeshObstacle>(); 
        ctx = GetComponentInParent<AIContext>(); 
        gs = GuestSettings.Instance; 
        if (obstacle != null) obstacle.enabled = false;
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null) return; 

        if (Time.time < movementTime)
        {
            if (agent.enabled) 
            {
                agent.enabled = false;
                if (obstacle != null) obstacle.enabled = true;      // become and obstacle if waiting 
            }
            return; 
        }
        
        // movement 
        if (!agent.enabled)
        {
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;

        }
        bool hasArrived = !agent.pathPending && agent.hasPath && (agent.remainingDistance <= gs.wanderMaxDistToDest);
        if (hasArrived)
        {
            float waitDuration = Random.Range(gs.wanderMinWaitAtNode, gs.wanderMaxWaitAtNode);
            movementTime = Time.time + waitDuration;

            agent.ResetPath(); 
            PickNextRandomNode();
            return; 
        }

        if (!agent.hasPath || ctx.forceNewPath)
        {
            ctx.forceNewPath = false; 

            if (ctx.targetNode == null) PickNextRandomNode();

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

    private void PickNextRandomNode()
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