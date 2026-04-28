using UnityEngine;
using UnityEngine.AI;

public class ConsiderationWanderFree : Consideration
{
    private NavMeshAgent agent;
    private AIContext ctx;
    
    // Global Settings and Weights
    private GuestWeights gw; 
    private MonsterWeights mw;
    private GuestSettings gs;
    private MonsterSettings ms;

    private float standingStartTime = -1f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance;
        mw = MonsterWeights.Instance;
        gs = GuestSettings.Instance;
        ms = MonsterSettings.Instance;
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || ctx.isOccupied) return 0f;

        // Once the guest NPC has realized it's being stalked, let ActionIsolate take over.
        if (!ctx.isMonster && ctx.isAwareOfStalker) return 0f;

        float maxBoredomTime = ctx.isMonster ? ms.idleMaxFatigue : gs.idleMaxFatigue;

        bool isStandingStill = !agent.isActiveAndEnabled || agent.velocity.sqrMagnitude < 0.1f;
        float dynamicScore = 0.1f;
        
        if (isStandingStill)
        {
            if (standingStartTime < 0f) standingStartTime = Time.time;
            float timeSpentStanding = Time.time - standingStartTime;
            dynamicScore = Mathf.Max(0.1f, Mathf.Clamp01(timeSpentStanding / maxBoredomTime));
        }
        else
        {
            standingStartTime = -1f;
        }
        float staticWeightMultiplier = ctx.isMonster ? mw.wanderFreeWeight : gw.soloWeight;

        return dynamicScore * staticWeightMultiplier;
    }
}