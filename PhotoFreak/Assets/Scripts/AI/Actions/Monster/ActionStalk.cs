using UnityEngine;
using UnityEngine.AI;

public class Action_Stalk : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private MonsterSettings ms; 
    private AIBrain brain; 
    private NPCIdentity identity; 
    
    void Awake() 
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        brain = GetComponentInParent<AIBrain>(); 
        identity = GetComponentInParent<NPCIdentity>(); 
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim is null || ctx.currentVictim.isMonster)
        {
            ctx.currentVictim = null;
            return;
        }

        ctx.currentVictim.isBeingStalked = true; 

        if (identity is not null) identity.ShowGuestModel(); 

        agent.isStopped = false;

        // move to the current victim 
        Vector3 preyRear = ctx.currentVictim.transform.position - (ctx.currentVictim.transform.forward * ms.stalkDistance);
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(preyRear, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float dist = Vector3.Distance(transform.position, ctx.currentVictim.transform.position); 

        if (brain is not null) ctx.currentStalkTimer += brain.decisionInterval; 
    }
}