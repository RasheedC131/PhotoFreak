using UnityEngine;

public class Consideration_TellCooldown : Consideration
{
    private float timer = 0f; 
    private MonsterSettings ms; 

    void Awake()
    {
        ms = MonsterSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        timer += Time.deltaTime; 
        float tellTime = Random.Range(ms.tellTimerMinTime, ms.tellTimerMaxTime); 
        float score = Mathf.Clamp01(timer / tellTime);

        return score; 
    }

    public void ResetTimer()
    {
        timer = 0f; 
    }
}