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
        if (ctx.currentVictim == null || ctx.currentVictim.isMonster)
        {
            ctx.currentVictim = null;
            return;
        }

        ctx.currentVictim.isBeingStalked = true; 
        agent.isStopped = false;

        // attack phase
        if (ctx.currentStalkTimer >= ctx.stalkDuration)
        {
            if (identity != null) identity.ShowMonsterModel(); 
            
            // Strike Mode: Zero out the stopping distance to get into attack range
            agent.stoppingDistance = 0.5f; 
            agent.SetDestination(ctx.currentVictim.transform.position); 
        }
        
        // stalk phase
        else
        {
            if (identity != null) identity.ShowGuestModel(); 
            
            float currentDist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position);

            if (currentDist < ms.stalkDistance - 1.0f)
            {
                agent.stoppingDistance = 0f;
                Vector3 dirAwayFromPrey = (ctx.transform.position - ctx.currentVictim.transform.position).normalized;
                Vector3 retreatPos = ctx.transform.position + (dirAwayFromPrey * 2.0f);
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(retreatPos, out hit, 2.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                agent.stoppingDistance = ms.stalkDistance;
                agent.SetDestination(ctx.currentVictim.transform.position);
            }
        }
        
        if (brain != null) ctx.currentStalkTimer += brain.decisionInterval; 
    }

    public override void OnExit()
    {
        if (agent != null) agent.stoppingDistance = 0f; 
    }
}