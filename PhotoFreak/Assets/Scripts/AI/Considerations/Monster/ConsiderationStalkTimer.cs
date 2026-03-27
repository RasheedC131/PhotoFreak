using UnityEngine;

public class ConsiderationStalkTimer : Consideration
{
    private AIContext ctx; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f; 

        if (ctx.currentVictim is null) return 1.0f; 

        float timeRatio = ctx.currentStalkTimer / ctx.stalkDistance; 

        // score scales with time that the monster is hunting the target (e.g. if it is hunting for a while then it starts to lose interest if it can't land a kill)
        float score = 1.0f - timeRatio; 

        return Mathf.Clamp(score, 0.2f, 1.0f); 
    }
}
