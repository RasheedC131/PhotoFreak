using UnityEngine; 

public class ConsiderationTellCooldown : Consideration
{
    private AIContext ctx;
    private float timerStartTime = -1f;
    private float currentTellTime = 0f;
    private MonsterSettings ms;
    private ActionTriggerTell tellAction;
    private NPCIdentity identity;

    void Awake()
    {
        ctx      = GetComponentInParent<AIContext>();
        ms       = MonsterSettings.Instance;
        tellAction = GetComponent<ActionTriggerTell>();
        identity = GetComponentInParent<NPCIdentity>();

        if (ms == null) Debug.LogError("MonsterSettings missing on " + gameObject.name);
        if (tellAction == null) Debug.LogError("ActionTriggerTell missing on " + gameObject.name);

        NewThreshold();
    }

    protected override float EvaluateRawValue()
    {
        if (ms == null || ctx == null || !ctx.isMonster) return 0f;

        // Tells only make sense while wearing the guest disguise.
        if (identity != null && !identity.isDisguised) return 0f;

        if (ctx.currentVictim != null && ctx.currentStalkTimer >= ms.stalkDuration) return 0f;

        if (tellAction != null && tellAction.isPerformingTell) return 5.0f;

        if (timerStartTime < 0f) timerStartTime = Time.time;
        float elapsed = Time.time - timerStartTime;

        if (elapsed >= currentTellTime)
        {
            Debug.Log($"[{gameObject.name}] Tell Timer Finished! Requesting Tell Action...");
            return 5.0f; 
        }

        return 0f; 
    }

    public void ResetTimer()
    {
        timerStartTime = Time.time; // Safer to reset to current time than -1f
        NewThreshold();
    }

    private void NewThreshold()
    {
        if (ms == null) return;
        currentTellTime = Random.Range(ms.tellTimerMinTime, ms.tellTimerMaxTime);
    }
}