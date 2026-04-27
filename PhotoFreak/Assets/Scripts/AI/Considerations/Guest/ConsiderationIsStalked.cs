using UnityEngine;

public class ConsiderationIsStalked : Consideration
{
    private AIContext ctx;
    private GuestWeights gw;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw  = GuestWeights.Instance;
    }

    protected override float EvaluateRawValue()
    {
        // Not being stalked — reset awareness and score zero.
        if (ctx == null || !ctx.isBeingStalked || ctx.currentStalker == null)
        {
            ctx.isAwareOfStalker = false;
            return 0f;
        }

        // All kill nodes are taken — nowhere to go, keep wandering.
        if (!KillNodeRegistry.HasAvailableNode(ctx))
        {
            ctx.isAwareOfStalker = false;
            return 0f;
        }

        // Stalked and a slot is free: set awareness immediately so every
        // competing consideration zeros itself out this same tick.
        ctx.isAwareOfStalker = true;
        return gw.isStalkedWeight;
    }
}