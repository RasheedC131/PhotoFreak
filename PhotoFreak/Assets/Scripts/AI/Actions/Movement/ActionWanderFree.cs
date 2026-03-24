using UnityEngine;
using UnityEngine.AI;

// pick a random spot to travel to on the navmesh 
public class ActionWanderFree : UtilityAction
{
    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 

    [Header("Solo Wander Settings")]
    [SerializeField] private float freeWanderRadius = 10.0f; 
    [SerializeField] private float arrivalDistance = 1.5f; 

    void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 
    }

    public override void ExecuteAction()
    {
        agent.isStopped = false; 
        float distToDest = Vector3.Distance(transform.position, ctx.currentDestination);    

        if (ctx.forceNewPath || distToDest < arrivalDistance)
        {
            ctx.forceNewPath = false; 

            Vector3 randomDirection = Random.insideUnitSphere * freeWanderRadius; 
            randomDirection += transform.position; 

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, freeWanderRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                ctx.currentDestination = hit.position; 
                agent.SetDestination(ctx.currentDestination); 
            }
        }    
    }
}
