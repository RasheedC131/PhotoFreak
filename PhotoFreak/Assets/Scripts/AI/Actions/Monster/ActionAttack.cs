using UnityEngine;
using UnityEngine.AI;

public class ActionAttack : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private MonsterSettings ms; 
    private NPCIdentity myIdentity; 
    
    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        myIdentity = GetComponentInParent<NPCIdentity>();
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim == null || ctx.currentVictim.isMonster) return;

        agent.isStopped = false;
        if (myIdentity != null) myIdentity.ShowMonsterModel();

        // Free the victim's reserved kill node immediately so the next stalked
        // guest can claim it right away, and clear the arrival flag so it does
        // not persist onto the newly-infected monster.
        ActionIsolate victimIsolate = ctx.currentVictim.GetComponentInChildren<ActionIsolate>(true);
        if (victimIsolate != null) victimIsolate.ReleaseKillNode();
        ctx.currentVictim.hasArrivedAtKillNode = false;

        if (MatchManager.Instance != null)
        {
            ctx.currentVictim.currentStalker = null;
            MatchManager.Instance.HandleInfection(ctx.currentVictim, ctx);
        }
        else
        {
            Debug.LogError("MatchManager Instance not found!");
        }

        Debug.Log($"Monster: [{ctx.gameObject.name}] infected: [{ctx.currentVictim.gameObject.name}]");

        ctx.currentVictim     = null;
        ctx.currentStalkTimer = 0f;
    }
}