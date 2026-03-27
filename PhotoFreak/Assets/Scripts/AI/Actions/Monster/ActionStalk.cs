using UnityEngine;
using UnityEngine.AI;

public class Action_Stalk : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private AIBrain brain; 
    private NPCIdentity identity; 
    
    [Header("Stalking Settings")]
    public float stalkDistance = 4.0f; 
    public float attackRange = 1.5f; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
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

        if (identity is not null) identity.ShowGuestModel(); 

        agent.isStopped = false;

        // move to the current victim 
        Vector3 preyRear = ctx.currentVictim.transform.position - (ctx.currentVictim.transform.forward * stalkDistance);
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(preyRear, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = 2.5f; 
        }

        float dist = Vector3.Distance(transform.position, ctx.currentVictim.transform.position); 

        if (brain is not null) ctx.currentStalkTimer += brain.decisionInterval; 

        if (ctx.currentStalkTimer >= ctx.stalkDuration)
        {
            Debug.Log("Monster timed out stalking, looking for new victim..."); 
            ctx.currentVictim = null; 
            ctx.currentStalkTimer = 0f; 
        }
    }
}