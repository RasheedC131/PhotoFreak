 using UnityEngine;

public class Consideration_GroupAvailable : Consideration
{
    private AIContext context;
    private GuestWeights gw; 
    
    [Header("Social Fatigue")]
    [Tooltip("How long an NPC will socialize before getting completely bored and leaving.")]
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

        // fatigue/boredom 
        if (context.isOccupied && context.targetHub != null)
        {
            if (socializingStartTime < 0f) socializingStartTime = Time.time;
            
            float timeSpent = Time.time - socializingStartTime;
            int totalPeople = context.targetHub.CurrentAttendees + context.targetHub.IncomingAttendees;
            float boredomSpeedMultiplier = (totalPeople <= 1) ? 4.0f : 1.0f; 
            float boredomDecay = Mathf.Clamp01(timeSpent * boredomSpeedMultiplier / maxSocializeTime);
            float currentScore = 1.0f - boredomDecay;

            if (currentScore <= 0.1f)
            {
                socializingStartTime = -1f; 
                return 0f; 
            }

            return currentScore;
        }

        socializingStartTime = -1f;
        if (context.isOccupied) return 0f; 

        if (context.targetNode != null)
        {
            ActionWanderNodes wanderAction = GetComponent<ActionWanderNodes>();
            if (wanderAction != null && !wanderAction.IsWaiting()) return 0f;
        }

        if (SocialHubManager.Instance == null || SocialHubManager.Instance.activeHubs.Count == 0) return 0f;

        foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
        {
            if (hub != null && hub.HasOpenSlots())
            {
                float dist = Vector3.Distance(context.transform.position, hub.transform.position);
                if (dist <= 15.0f) return gw.groupAvailWeight; 
            }
        }

        return 0f; 
    }
}