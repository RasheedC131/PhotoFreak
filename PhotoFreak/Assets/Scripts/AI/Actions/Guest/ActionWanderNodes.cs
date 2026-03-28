using UnityEngine;

// get a random group of npcs and pick a node to travel to on the navmesh 
public class ActionWanderNodes : UtilityAction
{

    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 
    private float MAX_DISTANCE_TO_DEST = 1.5f; 
    // private float MIN_WALK_OFFSET = -2.0f; 
    // private float MAX_WALK_OFFSET = 2.0f; 

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
        ctx = GetComponent<AIContext>(); 
    }

    public override void ExecuteAction()
    {
        agent.isStopped = false; 
        if (ctx.targetNode is not null)
        {
            float distToDest = Vector3.Distance(transform.position, ctx.currentDestination); 

            if (ctx.forceNewPath || distToDest < MAX_DISTANCE_TO_DEST)
            {
                if (!ctx.forceNewPath)
                {
                    ZoneNode script = ctx.targetNode.GetComponent<ZoneNode>(); 
                    if (script is not null) ctx.targetNode = script.NextNode; 
                }

                ctx.forceNewPath = false; 

                // provide an offset so they don't walk in a perfect straight line to the next targetNode 
                // Vector3 randomOffset = new Vector3(
                //     Random.Range(MIN_WALK_OFFSET, MAX_WALK_OFFSET), 
                //     0, 
                //     Random.Range(MIN_WALK_OFFSET, MAX_WALK_OFFSET)); 

                Vector3 randomOffset = Random.insideUnitSphere * 2.0f; 
                randomOffset.y = 0; 

                // ctx.currentDestination = ctx.targetNode.position + randomOffset;
                agent.SetDestination(ctx.targetNode.position + randomOffset);
            }
        }
    }

}
