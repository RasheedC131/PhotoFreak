using UnityEngine; 

public class ConsiderationTellCooldown : Consideration
{
    private AIContext ctx;
    private float timerStartTime = -1f;
    private float currentTellTime = 0f;
    private MonsterSettings ms;
    private ActionTriggerTell tellAction;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance;
        tellAction = GetComponent<ActionTriggerTell>();

        if (ms == null) Debug.LogError("MonsterSettings missing on " + gameObject.name);
        if (tellAction == null) Debug.LogError("ActionTriggerTell missing on " + gameObject.name);

        NewThreshold();
    }

    protected override float EvaluateRawValue()
    {
        if (ms == null || ctx == null || !ctx.isMonster) return 0f;

        // Optional: Do not trigger a tell if we are actively charging the prey for a kill
        if (ctx.currentVictim != null && ctx.currentStalkTimer >= ctx.stalkDuration) return 0f;

        // 1. THE LOCK-IN: Protect the coroutine while it runs with a massive score
        if (tellAction != null && tellAction.isPerformingTell) return 5.0f;

        // 2. THE TIMER: Run the countdown
        if (timerStartTime < 0f) timerStartTime = Time.time;
        float elapsed = Time.time - timerStartTime;

        // 3. THE SPIKE: Force the AI Brain to switch
        if (elapsed >= currentTellTime)
        {
            // If you see this log, but the tell animation doesn't play, check your Inspector Action Weight!
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