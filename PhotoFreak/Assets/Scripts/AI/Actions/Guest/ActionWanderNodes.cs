using UnityEngine;

// get a random group of npcs and pick a node to travel to on the navmesh 
public class ActionWanderNodes : UtilityAction
{

    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 
    private GuestSettings gs; 

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
        ctx = GetComponent<AIContext>(); 
        gs = GuestSettings.Instance; 
    }

    public override void ExecuteAction()
    {
        agent.isStopped = false; 
        if (ctx.targetNode is not null)
        {
            float distToDest = Vector3.Distance(transform.position, ctx.currentDestination); 

            if (ctx.forceNewPath || distToDest < gs.isolateKillNodeArrivalDist)
            {
                if (!ctx.forceNewPath)
                {
                    ZoneNode script = ctx.targetNode.GetComponent<ZoneNode>(); 
                    if (script is not null) ctx.targetNode = script.NextNode; 
                }

                ctx.forceNewPath = false; 
                
                Vector3 randomOffset = Random.insideUnitSphere * 2.0f; 
                randomOffset.y = 0; 

                // ctx.currentDestination = ctx.targetNode.position + randomOffset;
                agent.SetDestination(ctx.targetNode.position + randomOffset);
            }
        }
    }

}
