using UnityEngine;
using UnityEngine.AI;

public class Action_Flee : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private GuestSettings gs; 
    

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        gs = GuestSettings.Instance; 
    }

    public override void ExecuteAction()
    {
        if (ctx.currentThreat == null) return;

        agent.isStopped = false;
        agent.speed = gs.fleePanicSpeed; 

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < 1.0f))
        {
            Vector3 directionAway = (ctx.transform.position - ctx.currentThreat.position).normalized;

            Vector3 targetPosition = ctx.transform.position + (directionAway * gs.fleeDistance);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 4.0f, NavMesh.AllAreas))
            {
                ctx.currentDestination = hit.position;
                agent.SetDestination(ctx.currentDestination);
            }
        }
    }
}