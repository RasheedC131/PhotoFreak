using UnityEngine;

public class ConsiderationWanderNodes : Consideration
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

        if (ctx.isOccupied && ctx.targetHub == null) ctx.isOccupied = false;
        

        ActionWanderNodes wanderAction = GetComponent<ActionWanderNodes>();
        
        if (wanderAction != null && wanderAction.IsWaiting()) return 1.0f;
        if (ctx.isOccupied) return 0f;   
        if (ctx.isMonster && ctx.currentVictim != null) return 0f;
        
        return gw.wanderNodesWeight; 
    }
}