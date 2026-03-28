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

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < arrivalDistance || ctx.forceNewPath))
        {
            ctx.forceNewPath = false; 

            Vector3 randomDirection = Random.insideUnitSphere * freeWanderRadius; 
            randomDirection += ctx.transform.position; 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, freeWanderRadius, NavMesh.AllAreas))
            {
                ctx.currentDestination = hit.position; 
                agent.SetDestination(ctx.currentDestination); 
            }
        }  
    }
}
