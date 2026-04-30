using UnityEngine;

// as time ticks down make the npcs more anxious and not want to form groups 
public class ConsiderationTimeAnxiety : Consideration
{
    private GuestSettings gs;

    void Awake()
    {
        gs = GuestSettings.Instance;
    }

    protected override float EvaluateRawValue()
    {
        if (Timer.MainInstance == null) return 1f;

        float timeRatio       = Timer.MainInstance.TimeRatio;
        float anxietyStart    = gs != null ? gs.timeAnxietyStartRatio    : 0.4f;
        float minSocialScore  = gs != null ? gs.timeAnxietyMinSocialScore : 0.5f;

        if (timeRatio >= anxietyStart) return 1f;
        float progress = 1f - (timeRatio / anxietyStart);
        return Mathf.Lerp(1f, minSocialScore, progress);
    }
}
