using UnityEngine;

public class ConsiderationIsSolo : Consideration
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
        if (ctx == null) return 0f;
        if (ctx.isOccupied || ctx.isMonster) return 0f;
        
        return gw.soloWeight;
    }
}
