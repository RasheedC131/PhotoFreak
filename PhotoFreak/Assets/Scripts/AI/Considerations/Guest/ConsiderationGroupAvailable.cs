using UnityEngine;

public class Consideration_GroupAvailable : Consideration
{
    private AIContext context;
    private GuestWeights gw; 
    
    [Header("Social Fatigue")]
    public float maxSocializeTime = 20.0f;
    private float socializingStartTime = -1f;

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
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
                
                int totalPeople = node.currentCrowd.Count + node.incomingCrowd.Count;
                float boredomSpeedMultiplier = (totalPeople <= 1) ? 4.0f : 1.0f; 
                float currentScore = 1.0f - Mathf.Clamp01(timeSpent * boredomSpeedMultiplier / maxSocializeTime);
                
                if (currentScore <= 0.1f)
                {
                    socializingStartTime = -1f; 
                    return 0f; 
                }
                return gw.socialWeight; 
            }
        }

        socializingStartTime = -1f;
        return 0f; 
    }
}