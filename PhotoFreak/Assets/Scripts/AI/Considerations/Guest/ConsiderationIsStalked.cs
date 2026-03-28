using UnityEngine;

public class ConsiderationIsStalked : Consideration
{
    private AIContext ctx; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is not null && ctx.isBeingStalked) return 0.9f;
        
        return 0f; 
    }
}
