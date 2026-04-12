using UnityEngine;
using UnityEngine.AI;

public class ActionWanderFree : UtilityAction
{
    private NavMeshAgent agent; 
    private NavMeshObstacle obstacle; 
    private AIContext ctx; 

    [Header("Solo Wander Settings")]
    [SerializeField] private float freeWanderRadius = 10.0f; 
    [SerializeField] private float arrivalDistance = 1.5f; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        obstacle = GetComponentInParent<NavMeshObstacle>(); 
        ctx = GetComponentInParent<AIContext>(); 
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null) return;

        if (ctx.currentActionState != NPCActionState.IDLE && ctx.currentActionState != NPCActionState.WALK)
        {
            ctx.currentActionState = NPCActionState.IDLE;
        }

        if (!agent.enabled)
        {
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;
        }

        if (agent.isOnNavMesh && agent.isStopped) agent.isStopped = false; 
        
        if (ctx.targetNode != null) ctx.targetNode = null;
        
        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < arrivalDistance || ctx.forceNewPath))
        {
            ctx.forceNewPath = false; 

            Vector3 randomDirection = Random.insideUnitSphere * freeWanderRadius; 
            randomDirection += ctx.transform.position; 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, freeWanderRadius, NavMesh.AllAreas))
            {
                ctx.currentDestination = hit.position; 
                if (agent.isOnNavMesh) agent.SetDestination(ctx.currentDestination); 
            }
        }  
    }
}