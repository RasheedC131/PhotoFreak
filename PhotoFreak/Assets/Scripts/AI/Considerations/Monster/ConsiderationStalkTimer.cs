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

        if (ctx.currentVictim is null) return 1.0f;     // prioritize stalking when we don't have a set victim

        float timeRatio = ctx.currentStalkTimer / ctx.stalkDuration;

         if (timeRatio >= 1.0f)
        {
            Debug.Log("Monster got bored and gave up stalking");
            ctx.currentVictim = null;
            ctx.currentStalkTimer = 0f;
            return 0f; 
        }

        // score scales with time that the monster is hunting the target (e.g. if it is hunting for a while then it starts to lose interest if it can't land a kill)
        float score = Mathf.Lerp(1.0f, mw.stalkMinWeight, timeRatio);

        return score;
    }
}
