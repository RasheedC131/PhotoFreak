using UnityEngine;

public class Consideration_TellCooldown : Consideration
{
    private AIContext context;

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
    }

    void Update()
    {
        if (context.isMonster)context.currentStalkTimer += Time.deltaTime;
    }

    protected override float EvaluateRawValue()
    {
        if (context.currentStalkTimer >= context.stalkDuration) return 1f;
        return 0f; 
    }
}