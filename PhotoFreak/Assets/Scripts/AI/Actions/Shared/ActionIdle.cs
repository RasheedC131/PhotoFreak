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
        // Switch to NavMeshObstacle so this NPC becomes a hard blocker.
        // A stopped NavMeshAgent only creates soft RVO repulsion, meaning
        // moving agents can still drift through it under avoidance pressure.
        // As an obstacle, other agents path around it properly.
        // AIBrain.BrainTickRoutine already disables the obstacle and
        // re-enables the agent before any movement action begins, so the
        // transition back to mobility is handled automatically.
        if (agent.enabled)
        {
            if (agent.isOnNavMesh) agent.ResetPath();
            agent.enabled = false;
            if (obstacle != null) obstacle.enabled = true;
        }
    }
}