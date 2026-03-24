using UnityEngine;

public class Consideration_PreyNearby : Consideration
{
    private AIContext context;
    [SerializeField] private float sightRadius = 15f;

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
    }

    protected override float EvaluateRawValue()
    {
        if (context.currentVictim != null && !context.currentVictim.isMonster) return 1f;

        Collider[] hits = Physics.OverlapSphere(transform.position, sightRadius);
        
        float closestDistance = Mathf.Infinity;
        AIContext bestTarget = null;

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

        if (bestTarget != null)
        {
            context.currentVictim = bestTarget;
            return 1f; 
        }

        return 0f; 
    }
}