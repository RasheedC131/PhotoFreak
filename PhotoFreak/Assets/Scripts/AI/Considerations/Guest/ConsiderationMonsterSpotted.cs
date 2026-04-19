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
        if (ctx.isMonster) return 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, gw.guestPanicDistance);
        float highestPanic = 0f; 

        foreach (Collider hit in hits)
        {
            AIContext nearbyNPC = hit.GetComponent<AIContext>(); 

            if (nearbyNPC != null && nearbyNPC.isMonster)
            {
                ctx.currentVictim = nearbyNPC; 
                float dist = Vector3.Distance(transform.position, nearbyNPC.transform.position);
                float panicIntensity = 1.0f - (dist / gw.monsterSpottedWeight);                
                float score = panicIntensity * gw.monsterSpottedWeight;
                if (score > highestPanic) highestPanic = score;            
            }
        }

        return 0f; 
    }
}
