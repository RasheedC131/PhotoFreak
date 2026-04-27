using UnityEngine;

// see if victim is close enough and make sure no one is watching
public class ConsiderationInAttackRange : Consideration
{
    private AIContext ctx; 
    private MonsterSettings ms; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        ms = MonsterSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster || ctx.currentVictim is null) return 0f;

        bool isFinalPanic     = CrowdStateManager.Instance != null && CrowdStateManager.Instance.IsFinalPanic;
        bool victimAtKillNode = ctx.currentVictim.hasArrivedAtKillNode;


        if (!isFinalPanic && !victimAtKillNode && ctx.currentStalkTimer < ms.stalkDuration) return 0f;

        if (!IsAreaIsolated()) return 0f;

        // Always use attackRange for the distance check. killRoomAttackRange was
        // meant to be tighter but the NavMesh stoppingDistance (0.5 m) is path-
        // distance, not Euclidean — the agent can stop further than expected on
        // indirect routes, causing the tighter range to never fire.
        float dist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position);
        if (dist <= ms.attackRange) return 2.0f;

        return 0f;
    }

    private bool IsAreaIsolated()
    {
        bool    victimAtKillNode = ctx.currentVictim != null && ctx.currentVictim.hasArrivedAtKillNode;
        Vector3 checkOrigin = victimAtKillNode ? ctx.currentVictim.transform.position : ctx.transform.position;
        float   checkRadius = victimAtKillNode ? ms.killRoomWitnessRadius : ms.witnessRadius;

        Collider[] hits = Physics.OverlapSphere(checkOrigin, checkRadius);
        foreach (Collider hit in hits)
        {
            // Player proximity no longer blocks the actual attack — the monster
            // can infect even if the player is watching. The reveal in ActionStalk
            // still requires the player to be outside witnessRadius, so the
            // player gets a chance to intervene before the monster closes in.
            // Other monsters are allies, not witnesses — exclude them too.
            AIContext otherNPC = hit.GetComponentInParent<AIContext>();
            if (otherNPC != null && !otherNPC.isMonster && otherNPC != ctx && otherNPC != ctx.currentVictim)
                return false;
        }

        return true;
    }
}