using UnityEngine;
using UnityEngine.AI;

// monster has given up on hunting guest and will now focus completley on the player as a last resort
public class ActionHuntPlayer : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private MonsterSettings ms;
    private NPCIdentity identity;
    private Transform playerTransform;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        ms  = MonsterSettings.Instance;
        identity = GetComponentInParent<NPCIdentity>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    public override void OnEnter()
    {
        if (identity != null) identity.ShowMonsterModel();

        if (ctx.currentVictim != null)
        {
            ctx.currentVictim.isBeingStalked  = false;
            ctx.currentVictim.currentStalker  = null;
            ctx.currentVictim = null;
        }

        ctx.currentStalkTimer = 0f;

        Debug.Log($"[{ctx.gameObject.name}] Entering hunt mode — targeting the player.");
    }

    public override void ExecuteAction()
    {
        if (playerTransform == null) return;
        if (CrowdStateManager.Instance == null || !CrowdStateManager.Instance.IsFinalPanic) return;

        // Keep the monster model visible every tick in case another action changed it
        if (identity != null) identity.ShowMonsterModel();

        agent.isStopped = false;
        agent.speed     = ms.huntPlayerSpeed;

        if (agent.isOnNavMesh)
            agent.SetDestination(playerTransform.position);

        // TODO: player some sort of effect/cutscene or something 
        // Catch check — trigger game over when the monster reaches the player.
        float dist = Vector3.Distance(ctx.transform.position, playerTransform.position);
        if (dist <= ms.attackRange)
        {
            Debug.Log($"[{ctx.gameObject.name}] Caught the player! Game over.");
            if (GlobalGameState.Instance != null)
                GlobalGameState.Instance.TriggerGameOver();
        }
    }

    public override void OnExit()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }
    }
}
