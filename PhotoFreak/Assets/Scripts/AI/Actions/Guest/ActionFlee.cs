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
        if (ctx.currentVictim == null) return;

        agent.isStopped = false;
        
        Vector3 runDirection = transform.position - ctx.currentVictim.transform.position;
        Vector3 fleeTarget = transform.position + (runDirection.normalized * gs.fleeDistance);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, gs.fleeDistance, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        
    }
}