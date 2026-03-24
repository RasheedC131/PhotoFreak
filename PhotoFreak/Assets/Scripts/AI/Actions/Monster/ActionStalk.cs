using UnityEngine;
using UnityEngine.AI;

public class Action_Stalk : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    
    [Header("Stalking Settings")]
    public float stalkDistance = 4.0f; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim == null || ctx.currentVictim.isMonster)
        {
            ctx.currentVictim = null;
            return;
        }

        agent.isStopped = false;

        Vector3 preyRear = ctx.currentVictim.transform.position - (ctx.currentVictim.transform.forward * stalkDistance);
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(preyRear, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            
            agent.speed = 2.5f; 
        }
    }
}