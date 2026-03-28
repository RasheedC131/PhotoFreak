using UnityEngine;

public class ConsiderationPlayerSpotted : Consideration
{
    private AIContext ctx;
    private GuestWeights gw; 
    private GuestSettings gs; 
    private Transform playerTransform;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
        gs = GuestSettings.Instance; 

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    protected override float EvaluateRawValue()
    {

        if (ctx == null || playerTransform == null) return 0f;

        float dist = Vector3.Distance(ctx.transform.position, playerTransform.position);

        // TODO: implement player detection logic 
        if (dist <= gs.fleePlayerSightRadius)
        {
            ctx.currentThreat = playerTransform;
            return gw.playerSpottedWeight;
        }

        if (ctx.currentThreat == playerTransform) 
        {
            ctx.currentThreat = null;
        }
        
        return 0f;
    }
}
