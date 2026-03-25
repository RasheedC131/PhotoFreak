using UnityEngine;

public class Consideration_WanderNodes : Consideration
{
    private AIContext ctx;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
    }

    protected override float EvaluateRawValue() 
    {
        if (ctx == null) return 0f;

        // if they're busy stay in their position 
        if (ctx.isOccupied) return 0f;
        
        // if monster focus on the current victim 
        if (ctx.isMonster && ctx.currentVictim != null) return 0f;
        
        // assign a low score so that it can be changed by another event later 
        return 0.4f; 
    }
}