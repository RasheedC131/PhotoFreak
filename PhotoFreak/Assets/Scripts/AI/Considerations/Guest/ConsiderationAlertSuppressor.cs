using UnityEngine;

// supresses the socialize action when strikes go up
public class ConsiderationAlertSuppressor : Consideration
{
    private GuestSettings gs;

    void Awake()
    {
        gs = GuestSettings.Instance;
    }

    protected override float EvaluateRawValue()
    {
        if (CrowdStateManager.Instance == null) return 1f;

        float alert = CrowdStateManager.Instance.AlertLevel;
        float threshold = gs != null ? gs.alertSuppressThreshold : 0.45f;

        if (alert < threshold) return 1f;

        // Above threshold, linearly fade toward 0 as alert approaches 1
        float suppressionProgress = (alert - threshold) / (1f - threshold);
        return 1f - suppressionProgress;
    }
}
