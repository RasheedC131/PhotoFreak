using UnityEngine;

// get a random group of npcs and pick a node to travel to on the navmesh 
public class ActionWanderNodes : UtilityAction
{

    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 
    private GuestSettings gs; 

    private float movementTime = 0f; 

    void Start()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>(); 
        ctx = GetComponent<AIContext>(); 
        gs = GuestSettings.Instance; 
    }

    public override void ExecuteAction()
    {
        if (ctx.targetNode == null || ctx == null || agent == null) return; 

        if (Time.time < movementTime)
        {
            agent.isStopped = true;
            return; 
        }
        
        agent.isStopped = false; 
        float distToDest = Vector3.Distance(transform.position, ctx.currentDestination); 

        if (ctx.forceNewPath || distToDest < gs.wanderMaxDistToDest)
        {
            if (!ctx.forceNewPath)
            {
                ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>(); 
                if (nodeScript != null) ctx.targetNode = nodeScript.GetRandomNeighbor(ctx.transform); 
            }

            ctx.forceNewPath = false; 
            
            float waitDuration = Random.Range(gs.wanderMinWaitAtNode, gs.wanderMaxWaitAtNode);
            movementTime = Time.time + waitDuration;
            Vector3 randomOffset = Random.insideUnitSphere * gs.wanderNodeSpreadRadius; 
            randomOffset.y = 0; 

            agent.SetDestination(ctx.targetNode.position + randomOffset);        }
    }
    

}
