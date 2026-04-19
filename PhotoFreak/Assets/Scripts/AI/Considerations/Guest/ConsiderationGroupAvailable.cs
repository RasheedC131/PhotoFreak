using UnityEngine;

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

        if (context.targetNode != null)
        {
            ZoneNode node = context.targetNode.GetComponent<ZoneNode>();

            if (node != null && node.currentCrowd.Contains(context))
            {
                if (socializingStartTime < 0f) socializingStartTime = Time.time;
                float timeSpent = Time.time - socializingStartTime;

                // Hold at full weight during minimum stay
                if (timeSpent < gs.socialMinTime)
                    return gw.socialWeight;

                int totalPeople = node.currentCrowd.Count + node.incomingCrowd.Count;
                float boredomSpeedMultiplier = (totalPeople <= 1) ? 4.0f : 1.0f;

                // Decay after minimum time, over socialTime duration
                float decayProgress = Mathf.Clamp01((timeSpent - gs.socialMinTime) / gs.socialTimeDecay * boredomSpeedMultiplier);
                float currentScore = (1.0f - decayProgress) * gw.socialWeight;

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