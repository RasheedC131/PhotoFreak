using UnityEngine;

public class ConsiderationWanderNodes : Consideration
{
    private AIContext ctx;
    private GuestWeights gw; 
    private ActionWanderNodes wanderAction;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
        wanderAction = GetComponent<ActionWanderNodes>();
    }

    protected override float EvaluateRawValue() 
    {
        if (ctx == null) return 0f;

        if (ctx.isMonster && ctx.currentVictim != null) return 0f;
        
        if (ctx.isOccupied) return 0f;   

        if (wanderAction != null && wanderAction.IsWaiting()) return 1.0f;
        
        if (ctx.targetNode == null)
        {
            if (wanderAction != null && !wanderAction.HasOpenNodes())
            {
                return 0f; 
            }
        }

        return gw.wanderNodesWeight; 
    }
}