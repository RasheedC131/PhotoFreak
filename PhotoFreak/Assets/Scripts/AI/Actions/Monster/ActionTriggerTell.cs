using UnityEngine;
using UnityEngine.AI;

public class ActionTriggerTell : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;

    [Header("Tell Settings")]
    [SerializeField] private float tellDuration = 2.0f;
    private float tellTimer;
    private bool isPreformingTell = false;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (!isPreformingTell)
        {
            // Start the glitch
            isPreformingTell = true;
            tellTimer = tellDuration;
            agent.isStopped = true;

            // TODO: Implement tells with monster animations 
            Debug.Log($"{ctx.gameObject.name} is performing a monster tell!");
        }
        else
        {
            tellTimer -= Time.deltaTime;
            
            if (tellTimer <= 0)
            {
                isPreformingTell = false;
                ctx.currentStalkTimer = 0f; 
                agent.isStopped = false;
            }
        }
    }
}