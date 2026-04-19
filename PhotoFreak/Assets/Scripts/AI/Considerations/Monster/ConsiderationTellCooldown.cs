using UnityEngine; 

public class ConsiderationTellCooldown : Consideration
{
    private float timerStartTime = -1f;
    private float currentTellTime = 0f;
    private MonsterSettings ms;
    private ActionTriggerTell tellAction;

    void Awake()
    {
        ms = MonsterSettings.Instance;
        tellAction = GetComponent<ActionTriggerTell>();

        if (ms == null) Debug.LogError("MonsterSettings not found in Resources/AI/ on " + gameObject.name);
        if (tellAction == null) Debug.LogError("ActionTriggerTell not found on " + gameObject.name);

        NewThreshold();
    }

    protected override float EvaluateRawValue()
    {
        if (ms == null) return 0f;
        

        if (tellAction != null && tellAction.isPerformingTell) return 1.5f;

        if (timerStartTime < 0f) timerStartTime = Time.time;
        float elapsed = Time.time - timerStartTime;

        if (elapsed >= currentTellTime) return 1.5f; 
        
        return 0f; 
    }

    public void ResetTimer()
    {
        timerStartTime = -1f;
        NewThreshold();
    }

    private void NewThreshold()
    {
        if (ms == null) return;
        currentTellTime = Random.Range(ms.tellTimerMinTime, ms.tellTimerMaxTime);
    }
}