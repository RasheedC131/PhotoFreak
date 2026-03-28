using UnityEngine;

public class ConsiderationInAttackRange : Consideration
{
    private AIContext ctx; 

    [Header("Attack Settings")]
    public float attackRange = 2.5f; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx is null || !ctx.isMonster || ctx.currentVictim is null) return 0f; 

        float dist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position); 

        if (dist <= attackRange) return 1.0f; 
        
        return 0f; 
    }
}
