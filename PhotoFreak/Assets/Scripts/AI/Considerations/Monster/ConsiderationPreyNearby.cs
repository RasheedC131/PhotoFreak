using UnityEngine;

// belongs to the stalking action 
public class Consideration_PreyNearby : Consideration
{
    private AIContext ctx;
    [SerializeField] private float sightRadius = 15f;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster) return 0f; 
        if (ctx.currentVictim is not null && !ctx.currentVictim.isMonster) return 0.8f;

        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, sightRadius);
        
        float closestDistance = Mathf.Infinity;
        AIContext bestTarget = null;

        // TODO: Tweak this logic to take in more factors to select the best possible target 
        foreach (Collider hit in hits)
        {
            AIContext potentialPrey = hit.GetComponent<AIContext>();
            
            if (potentialPrey != null && !potentialPrey.isMonster)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
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

    void OnDrawGizmosSelected()
    {
        if (ctx != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ctx.transform.position, sightRadius);
        }
    }
}