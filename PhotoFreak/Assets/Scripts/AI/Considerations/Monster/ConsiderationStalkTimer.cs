using UnityEngine;

public class ConsiderationStalkTimer : Consideration
{
    private AIContext ctx; 
    private MonsterWeights mw; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        mw = MonsterWeights.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f; 

        if (ctx.currentVictim is null) return 1.0f;    

        float timeRatio = ctx.currentStalkTimer / ctx.stalkDuration;

        if (timeRatio >= 3.0f)
        {
            Debug.Log("Monster got bored and gave up chasing");
            ctx.currentVictim.isBeingStalked = false; 
            ctx.currentVictim = null;
            ctx.currentStalkTimer = 0f;
            return 0f; 
        }

        // Keeps the score strong enough to maintain the stalk/charge state
        return Mathf.Lerp(1.0f, mw.stalkMinWeight, timeRatio / 3.0f);
    }
}