using UnityEngine;
using UnityEngine.AI;

public class Action_Stalk : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private AIBrain brain; 
    
    [Header("Stalking Settings")]
    public float stalkDistance = 4.0f; 
    public float attackRange = 1.5f; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        brain = GetComponentInParent<AIBrain>(); 
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim is null || ctx.currentVictim.isMonster)
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

        float dist = Vector3.Distance(transform.position, ctx.currentVictim.transform.position); 

        if (dist <= attackRange)
        {
            Debug.Log($"<color=red> MONSTER KILLED: {ctx.currentVictim.gameObject.name}!</color>");
            ctx.currentVictim = null; 
            return; 
        }

        if (brain is not null) ctx.currentStalkTimer += brain.decisionInterval; 

        if (ctx.currentStalkTimer >= ctx.stalkDuration)
        {
            Debug.Log($"Monster gave up current target: {ctx.currentVictim.gameObject.name}"); 
            ctx.currentVictim = null; 
            ctx.currentStalkTimer = 0f; 
        }
    }
}