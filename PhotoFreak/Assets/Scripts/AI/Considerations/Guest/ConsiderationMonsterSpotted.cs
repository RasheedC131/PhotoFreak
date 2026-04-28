using UnityEngine;

public class ConsiderationMonsterSpotted : Consideration
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
        if (ctx == null || ctx.isMonster) return 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, gw.guestPanicDistance);
        float highestPanic = 0f;

        foreach (Collider hit in hits)
        {
            AIContext nearbyNPC = hit.GetComponentInParent<AIContext>();
            if (nearbyNPC == null || !nearbyNPC.isMonster) continue;

            // Only flee from a revealed monster — a disguised one looks like a normal guest.
            NPCIdentity identity = nearbyNPC.GetComponent<NPCIdentity>();
            if (identity != null && identity.isDisguised) continue;

            float dist  = Vector3.Distance(transform.position, nearbyNPC.transform.position);
            float intensity = Mathf.Clamp01(1.0f - (dist / gw.guestPanicDistance));
            float score = intensity * gw.monsterSpottedWeight;

            if (score > highestPanic)
            {
                highestPanic       = score;
                ctx.currentThreat  = nearbyNPC.transform;
            }
        }

        if (highestPanic <= 0f && ctx.currentThreat != null)
        {
            // Threat left the detection radius — clear it.
            AIContext threatCtx = ctx.currentThreat.GetComponent<AIContext>();
            if (threatCtx != null && threatCtx.isMonster)
                ctx.currentThreat = null;
        }

        return highestPanic;
    }
}
