using UnityEngine;

public class ConsiderationIsStalked : Consideration
{
    private AIContext ctx; 
    private GuestWeights gw; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        gw = GuestWeights.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is not null && ctx.isBeingStalked) return gw.isStalkedWeight;
        
        return 0f; 
    }
}
