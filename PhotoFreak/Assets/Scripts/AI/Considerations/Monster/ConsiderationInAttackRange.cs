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

        if (ctx.currentStalkTimer < ms.stalkDuration) return 0f;
        
        if (!IsAreaIsolated()) return 0f;

        float dist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position); 
        if (dist <= ms.attackRange) return 2.0f; 
        
        return 0f; 
    }

    private bool IsAreaIsolated()
    {
        
        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, ms.witnessRadius);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) return false; 

            AIContext otherNPC = hit.GetComponentInParent<AIContext>();
            if (otherNPC != null && otherNPC != ctx && otherNPC != ctx.currentVictim)
            {
                return false; 
            }
        }

        return true; // No witnesses! Safe to kill.
    }
}