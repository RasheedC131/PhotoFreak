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

        // Suppress panic-based player flee only after the NPC has realised it's
        // being stalked and is heading to the kill room. Pre-threshold, panic
        // still fires normally so regular crowd behaviour is unaffected.
        if (ctx.panicBoost >= gs.panicFleeThreshold && !ctx.isAwareOfStalker)
        {
            if (ctx.currentThreat == null) ctx.currentThreat = playerTransform;
            return ctx.panicBoost * gw.playerSpottedWeight;
        }

        CrowdStateManager crowd = CrowdStateManager.Instance;
        float alertLevel = crowd != null ? crowd.AlertLevel : 0f;

        // If the player has accrued strikes, the crowd manager dictates an absolute
        // avoidance radius (5m / 10m / "close"). Use that directly so the spec's
        // distances are honored regardless of vigilance/alert pile-on. Pre-strike
        // we keep the original organic-feeling formula.
        float strikeRadius = crowd != null ? crowd.PlayerAvoidRadius : 0f;
        float effectiveRadius = strikeRadius > 0f
            ? strikeRadius
            : gs.fleePlayerSightRadius * (1f + ctx.vigilance * gs.vigilanceRadiusMultiplier) * (1f + alertLevel * gs.alertRadiusMultiplier);

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
