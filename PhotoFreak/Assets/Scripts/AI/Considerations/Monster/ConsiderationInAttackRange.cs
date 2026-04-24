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
            if (hit.CompareTag("Player")) return false;

            AIContext otherNPC = hit.GetComponentInParent<AIContext>();
            if (otherNPC != null && otherNPC != ctx && otherNPC != ctx.currentVictim)
                return false;
        }

        return true; 
    }
}