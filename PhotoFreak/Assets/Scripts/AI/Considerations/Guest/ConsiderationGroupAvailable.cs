using UnityEngine;

// See if the npc is able to join/form a group around a zone node place on the map 
public class Consideration_GroupAvailable : Consideration
{
    private AIContext context;
    private GuestWeights gw; 
    private GuestSettings gs; 
    
    [Header("Social Fatigue")]
    private float socializingStartTime = -1f;

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
        gs = GuestSettings.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        if (context == null || context.isMonster) return 0f;

        // if there's high tension like freak meter going off too much or monster is spotted by the guest reduces the will to socialize at the part 
        float alertLevel = CrowdStateManager.Instance != null ? CrowdStateManager.Instance.AlertLevel : 0f;
        if (alertLevel >= gs.alertSuppressThreshold)
        {
            if (context.targetNode == null) return 0f;
        }

        if (context.targetNode != null)
        {
            ZoneNode node = context.targetNode.GetComponent<ZoneNode>();

            if (node != null && node.currentCrowd.Contains(context))
            {
                if (socializingStartTime < 0f) socializingStartTime = Time.time;
                float timeSpent = Time.time - socializingStartTime;

                float effectiveSocialMinTime = alertLevel >= gs.alertSuppressThreshold ? 0f : gs.socialMinTime;

                if (timeSpent < effectiveSocialMinTime)
                    return gw.socialWeight;

                int totalPeople = node.currentCrowd.Count + node.incomingCrowd.Count;

        
                float boredomSpeedMultiplier = (totalPeople <= 1) ? 4.0f : 1.0f;
                boredomSpeedMultiplier *= (1f + alertLevel * gs.groupAlertBoredomMultiplier);

                float decayProgress = Mathf.Clamp01((timeSpent - effectiveSocialMinTime) / gs.socialTimeDecay * boredomSpeedMultiplier);
                float currentScore  = (1.0f - decayProgress) * gw.socialWeight;

                if (currentScore <= 0.1f)
                {
                    socializingStartTime = -1f;
                    return 0f;
                }

                return currentScore;
            }
        }

        socializingStartTime = -1f;
        return 0f;
    }
}