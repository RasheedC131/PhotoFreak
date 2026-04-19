using UnityEngine;

public class ConsiderationWanderNodes : Consideration
{
    private AIContext ctx;
    private GuestWeights gw; 
    private float personalityOffset;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
        if (gw != null) personalityOffset = Random.Range(-gw.maxPersonalityOffset, gw.maxPersonalityOffset);
    }

    protected override float EvaluateRawValue() 
    {
        if (ctx == null || (ctx.isMonster && ctx.currentVictim != null) || ctx.isOccupied) return 0f;

        if (ctx.targetNode != null || ctx.forceNewPath)
        {
            if (ctx.targetNode != null)
            {
                ZoneNode node = ctx.targetNode.GetComponent<ZoneNode>();
                if (node != null && node.currentCrowd.Contains(ctx)) return 0f; 
            }
            
            return gw.wanderNodesCommittedWeight; 
        }

        ActionWanderNodes wanderAction = GetComponent<ActionWanderNodes>();
        if (wanderAction != null && !wanderAction.HasOpenNodes()) return 0f; 

        return Mathf.Clamp01(gw.wanderNodesWeight + personalityOffset);
    }
}