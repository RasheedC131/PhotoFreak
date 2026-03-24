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
        context = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (!isGlitching)
        {
            // Start the glitch
            isPreformingTell = true;
            tellTimer = glitchDuration;
            agent.isStopped = true;

            // TODO: Implement tells with monster animations 
            Debug.Log($"{context.gameObject.name} is performing a monster tell!");
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