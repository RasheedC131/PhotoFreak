using UnityEngine;
using UnityEngine.AI;

public class ActionAttack : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext context;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        context = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (context.currentVictim == null || context.currentVictim.isMonster)
        {
            context.currentVictim = null;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(context.currentVictim.transform.position);

        float distanceToPrey = Vector3.Distance(transform.position, context.currentVictim.transform.position);
        
        if (distanceToPrey <= attackRange)
        {
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.HandleInfection(context.currentVictim, context);
                
                context.currentVictim = null; 
            }
        }
    }
}