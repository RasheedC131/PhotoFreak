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

        NavMeshAgent preyAgent = ctx.currentVictim.GetComponent<NavMeshAgent>();
        Vector3 targetDestination;

        if (preyAgent != null && preyAgent.velocity.sqrMagnitude < 0.1f) targetDestination = ctx.currentVictim.transform.position;
        
        else
        {
            targetDestination = ctx.currentVictim.transform.position - (ctx.currentVictim.transform.forward * ms.stalkDistance);
        }
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetDestination, out hit, 2.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        

        if (brain is not null) ctx.currentStalkTimer += brain.decisionInterval; 
    }
}