using UnityEngine;

// belongs to the stalking action 
public class ConsiderationPreyNearby : Consideration
{
    private AIContext ctx;
    private MonsterSettings ms; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f; 
        if (ctx.currentVictim is not null && !ctx.currentVictim.isMonster) return 0.8f;

        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, ms.stalkSenseRadius);
        
        float closestDistance = Mathf.Infinity;
        AIContext bestTarget = null;

        // TODO: Tweak this logic to take in more factors to select the best possible target 
        foreach (Collider hit in hits)
        {
            AIContext potentialPrey = hit.GetComponentInParent<AIContext>();
            
            if (potentialPrey != null && !potentialPrey.isMonster)
            {
                float dist = Vector3.Distance(ctx.transform.position, potentialPrey.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = potentialPrey;
                }
            }
        }

        if (bestTarget is not null)
        {
            ctx.currentVictim = bestTarget;
            return 0.8f; 
        }

        return 0f; 
    }
}