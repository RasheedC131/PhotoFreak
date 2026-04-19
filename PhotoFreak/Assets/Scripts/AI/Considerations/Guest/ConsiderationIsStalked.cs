using UnityEngine;

public class ConsiderationIsStalked : Consideration
{
    private AIContext ctx; 
    private GuestWeights gw; 
    private MonsterSettings ms; 

    [Header("Paranoia Settings")]
    [Tooltip("Percentage of the monster's stalk timer required before the guest isolates (e.g., 0.7 = 70%)")]
    public float realizationThreshold = 0.7f;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        gw = GuestWeights.Instance; 
        ms = MonsterSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || !ctx.isBeingStalked || ctx.currentStalker == null) return 0f;

        AIContext stalker = ctx.currentStalker;
        if (ms.stalkDuration <= 0f) return gw.isStalkedWeight;

        float timeRatio = stalker.currentStalkTimer / ms.stalkDuration;

        if (timeRatio >= realizationThreshold) return gw.isStalkedWeight; 
        
        return 0f; 
    }
}