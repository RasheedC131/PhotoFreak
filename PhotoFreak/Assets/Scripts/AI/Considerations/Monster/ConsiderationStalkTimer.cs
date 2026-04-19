using UnityEngine;

public class ConsiderationStalkTimer : Consideration
{
    private AIContext ctx; 
    private MonsterWeights mw; 
    private MonsterSettings ms; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        mw = MonsterWeights.Instance; 
        ms = MonsterSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f; 

        if (ctx.currentVictim is null) return 1.0f;    

        float timeRatio = ctx.currentStalkTimer / ms.stalkDuration;

        bool isVictimIsolating = false;
        
        if (ctx.currentVictim.GetComponent<AIBrain>() != null)
        {
            AIBrain victimBrain = ctx.currentVictim.GetComponent<AIBrain>();
            if (victimBrain.currentAction is ActionIsolate) isVictimIsolating = true;
            
        }

        if (timeRatio >= 3.0f && !isVictimIsolating)
        {
            Debug.Log("Monster got bored and gave up chasing");
            ctx.currentVictim.isBeingStalked = false; 
            ctx.currentVictim.currentStalker = null; 
            ctx.currentVictim = null;
            ctx.currentStalkTimer = 0f;
            return 0f; 
        }

        float clampedRatio = Mathf.Clamp01(timeRatio / 3.0f);
        // Return a score that keeps the monster stalking (usually between 1.0 and stalkMinWeight)
        return Mathf.Lerp(1.0f, mw.stalkMinWeight, clampedRatio);
    }
}