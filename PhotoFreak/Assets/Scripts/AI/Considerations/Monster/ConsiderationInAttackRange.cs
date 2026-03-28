using UnityEngine;

public class ConsiderationInAttackRange : Consideration
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
        if (ctx is null || !ctx.isMonster || ctx.currentVictim is null) return 0f; 

        float dist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position); 

        if (dist <= ms.attackRange) return 1.0f; // go for the kill 
        
        return 0f; 
    }
}
