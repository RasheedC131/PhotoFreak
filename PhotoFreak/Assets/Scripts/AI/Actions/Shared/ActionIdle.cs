using UnityEngine;
using UnityEngine.AI;

public class ActionIdle : UtilityAction
{
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        obstacle = GetComponentInParent<NavMeshObstacle>();
    }

    public override void ExecuteAction()
    {
        if (!agent.enabled)
        {
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;
        }

        if (agent.isOnNavMesh && !agent.isStopped) agent.isStopped = true;
    }
}