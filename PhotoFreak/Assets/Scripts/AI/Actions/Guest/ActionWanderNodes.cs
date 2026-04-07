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

        if (IsWaiting())
        {
            if (agent.enabled) 
            {
                agent.enabled = false;
                if (obstacle != null) obstacle.enabled = true;      
            }

            if (ctx.targetNode != null)
            {
                Vector3 lookPos = ctx.targetNode.position;
                lookPos.y = transform.position.y; 
                Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f); 
            }
            return; 
        }
        
        if (!agent.enabled)
        {
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;
        }

        if (agent.isOnNavMesh && agent.isStopped) agent.isStopped = false;

        if (ctx.targetNode != null)
        {
            ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
            if (nodeScript != null && nodeScript.GetCurrentCrowd() >= gs.wanderNodeMaxCapacity)
            {
                AbandonNode();
                return; 
            }
        }

        bool hasArrived = !agent.pathPending && agent.hasPath && (agent.remainingDistance <= gs.wanderMaxDistToDest);
        if (hasArrived && ctx.targetNode != null)
        {
            float waitDuration = Random.Range(gs.wanderMinWaitAtNode, gs.wanderMaxWaitAtNode);
            movementTime = Time.time + waitDuration;

            if (agent.isOnNavMesh) agent.ResetPath(); 
            return; 
        }

        if ((!agent.hasPath && !agent.pathPending) || ctx.forceNewPath)
        {
            ctx.forceNewPath = false; 

            if (ctx.targetNode == null) PickNextRandomNode();

            if (ctx.targetNode == null)
            {
                AbandonNode();
                return;
            }

            Vector3 randomOffset = Random.insideUnitSphere * gs.wanderNodeSpreadRadius; 
            randomOffset.y = 0;
            Vector3 desiredDestination = ctx.targetNode.position + randomOffset;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredDestination, out hit, gs.wanderNodeSpreadRadius, NavMesh.AllAreas))
            {
                ctx.currentDestination = hit.position;
            }
            else
            {
                ctx.currentDestination = ctx.targetNode.position; 
            }

            if (agent.isOnNavMesh) agent.SetDestination(ctx.currentDestination);        
        }
    }

    public void AbandonNode()
    {
        ctx.targetNode = null;
        ctx.forceNewPath = true;
        
        movementTime = 0f; 
        
        if (!agent.enabled)
        {
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;
        }
        
        if (agent.isOnNavMesh) agent.ResetPath();
    }

    private void PickNextRandomNode()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        List<ZoneNode> validNodes = new List<ZoneNode>();

        foreach (ZoneNode node in allNodes)
        {
            if (ctx.targetNode != null && node.transform == ctx.targetNode) continue;

            if (node.GetCurrentCrowd() < gs.wanderNodeMaxCapacity)
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

    public bool IsWaiting()
    {
        return Time.time < movementTime && ctx.targetNode != null;
    }

    public bool HasOpenNodes()
    {
        if (gs == null) return false;
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        foreach (ZoneNode node in allNodes)
        {
            if (node.GetCurrentCrowd() < gs.wanderNodeMaxCapacity) return true;
        }
        return false;
    }

    public override void OnExit()
    {
        AbandonNode(); 
    }
}