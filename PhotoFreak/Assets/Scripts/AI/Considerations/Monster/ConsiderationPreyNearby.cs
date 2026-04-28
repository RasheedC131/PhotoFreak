using UnityEngine;

// belongs to the stalking action
public class ConsiderationPreyNearby : Consideration
{
    private AIContext ctx;
    private MonsterSettings ms;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms  = MonsterSettings.Instance;
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f;

        // In kill mode (strike 3) monsters are aggressive — bump base score so
        // stalk/attack outranks idle and tell behaviour.
        bool killMode = CrowdStateManager.Instance != null && CrowdStateManager.Instance.IsKillMode;
        float baseScore = killMode ? 1.0f : 0.8f;

        if (ctx.currentVictim is not null && !ctx.currentVictim.isMonster) return baseScore;

        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, ms.stalkSenseRadius);

        float    bestScore  = -1f;
        AIContext bestTarget = null;

        foreach (Collider hit in hits)
        {
            AIContext potentialPrey = hit.GetComponentInParent<AIContext>();
            if (potentialPrey == null || potentialPrey.isMonster) continue;
            if (potentialPrey.isBeingStalked && potentialPrey.currentStalker != ctx) continue;

            float dist = Vector3.Distance(ctx.transform.position, potentialPrey.transform.position);
            float proximityScore = 1f - Mathf.Clamp01(dist / ms.stalkSenseRadius);

            float panicBonus = potentialPrey.panicBoost * 0.5f;

            float totalScore = proximityScore + panicBonus;

            if (totalScore > bestScore)
            {
                bestScore  = totalScore;
                bestTarget = potentialPrey;
            }
        }

        if (bestTarget is not null)
        {
            ctx.currentVictim = bestTarget;
            return baseScore;
        }

        return 0f;
    }
}
