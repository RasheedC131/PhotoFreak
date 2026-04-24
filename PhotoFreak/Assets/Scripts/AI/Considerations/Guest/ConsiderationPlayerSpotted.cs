using UnityEngine;

// NPCs react to the freak meter of the player 
public class ConsiderationPlayerSpotted : Consideration
{
    private AIContext ctx;
    private GuestWeights gw;
    private GuestSettings gs;
    private Transform playerTransform;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw  = GuestWeights.Instance;
        gs  = GuestSettings.Instance;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || playerTransform == null) return 0f;

        if (ctx.panicBoost >= gs.panicFleeThreshold && !ctx.isBeingStalked)
        {
            if (ctx.currentThreat == null) ctx.currentThreat = playerTransform;
            return ctx.panicBoost * gw.playerSpottedWeight;
        }

        float alertLevel = CrowdStateManager.Instance != null ? CrowdStateManager.Instance.AlertLevel : 0f;

        float effectiveRadius = gs.fleePlayerSightRadius * (1f + ctx.vigilance*gs.vigilanceRadiusMultiplier) * (1f + alertLevel*gs.alertRadiusMultiplier);

        float dist = Vector3.Distance(ctx.transform.position, playerTransform.position);

        if (dist <= effectiveRadius)
        {
            ctx.currentThreat = playerTransform;
            float intensity = 1.0f - (dist / effectiveRadius);
            return intensity * gw.playerSpottedWeight;
        }

        if (ctx.currentThreat == playerTransform) ctx.currentThreat = null;
        return 0f;
    }
}
