using UnityEngine;

public class ActionStalk : UtilityAction
{
    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext blackboard; 

    void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        blackboard = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (blackboard.currentVictim is not null) 
        {
            agent.SetDestination(blackboard.currentVictim.transform.position);
        }
    }
}
