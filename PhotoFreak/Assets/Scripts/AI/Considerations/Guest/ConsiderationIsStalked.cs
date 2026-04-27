using UnityEngine;

public class ConsiderationIsStalked : Consideration
{
    private AIContext ctx; 
    private GuestWeights gw; 
    private MonsterSettings ms; 

    [Header("Paranoia Settings")]
    public float realizationThreshold = 0.7f;
    private float _exposureStartTime = -1f;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance;
        ms = MonsterSettings.Instance;
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || !ctx.isBeingStalked || ctx.currentStalker == null)
        {
            _exposureStartTime = -1f; // reset if stalking ends
            return 0f;
        }

        if (ms.stalkDuration <= 0f) return gw.isStalkedWeight;

        // Start the victim-side exposure clock the first tick stalking begins.
        if (_exposureStartTime < 0f) _exposureStartTime = Time.time;

        float timeRatio = (Time.time - _exposureStartTime) / ms.stalkDuration;
        if (timeRatio < realizationThreshold) return 0f;

        if (!KillNodeRegistry.HasAvailableNode(ctx)) return 0f; 
        
        return gw.isStalkedWeight;
    }
}