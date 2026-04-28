using UnityEngine;

public class ConsiderationMonsterSpotted : Consideration
{
    private AIContext ctx; 
    private GuestWeights gw; 
    [SerializeField] private float panicRadius = 10f; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        gw = GuestWeights.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx.isMonster) return 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, panicRadius);
        foreach (Collider hit in hits)
        {
            AIContext nearbyActor = hit.GetComponent<AIContext>();
            
            if (nearbyActor != null && nearbyActor.isMonster)
            {
                ctx.currentVictim = nearbyActor; 
                return gw.monsterSpottedWeight; 
            }
        }

        return 0f; 
    }
}
