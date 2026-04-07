using UnityEngine;
using UnityEngine.AI;

public class ConsiderationIdle : Consideration
{
    private NavMeshAgent agent;
    private AIContext ctx;
    
    private GuestWeights gw; 
    private MonsterWeights mw;
    private GuestSettings gs;
    private MonsterSettings ms;

    private float walkingStartTime = -1f; 

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

        if (ctx.isMonster && ctx.currentVictim != null)
        {
            walkingStartTime = -1f; 
            return 0f; 
        }

        float maxFatigueTime = ctx.isMonster ? ms.idleMaxFatigue : gs.idleMaxFatigue;

        bool isWalking = agent.isActiveAndEnabled && agent.velocity.sqrMagnitude > 0.1f;

        float dynamicScore = 0.1f; 
        if (isWalking)
        {
            if (walkingStartTime < 0f) walkingStartTime = Time.time;
            float timeSpentWalking = Time.time - walkingStartTime;
            dynamicScore = Mathf.Max(0.1f, Mathf.Clamp01(timeSpentWalking / maxFatigueTime));
        }
        else
        {
            walkingStartTime = -1f;
        }

        float staticWeightMultiplier = ctx.isMonster ? mw.idleWeight : gw.idleWeight;
        return dynamicScore * staticWeightMultiplier; 
    }
}