using UnityEngine;

public class ConsiderationIsSolo : Consideration
{
    private AIContext ctx; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null) return 0f;
        if (ctx.isOccupied || ctx.isMonster) return 0f;
        
        return 0.5f;
    }
}
