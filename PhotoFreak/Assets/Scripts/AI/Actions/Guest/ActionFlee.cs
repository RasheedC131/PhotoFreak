using UnityEngine;
using UnityEngine.AI;

public class Action_Flee : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext context;
    public float fleeDistance = 8f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        context = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (context.currentVictim == null) return;

        agent.isStopped = false;
        
        Vector3 runDirection = transform.position - context.currentVictim.transform.position;
        Vector3 fleeTarget = transform.position + (runDirection.normalized * fleeDistance);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, fleeDistance, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        
    }
}