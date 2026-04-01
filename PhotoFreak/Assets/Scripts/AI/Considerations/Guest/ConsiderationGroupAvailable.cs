using UnityEngine;

public class Consideration_GroupAvailable : Consideration
{
    private AIContext context;
    private GuestWeights gw; 

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance; 
    }

    protected override float EvaluateRawValue()
    {

        if (context.isOccupied && context.targetHub != null) return 1.0f;

        if (context.isOccupied || context.isMonster) return 0f;

        if (SocialHubManager.Instance == null || SocialHubManager.Instance.activeHubs.Count == 0) return 0f;

        foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
        {
            if (hub != null && hub.HasOpenSlots())
            {
      
                float dist = Vector3.Distance(context.transform.position, hub.transform.position);
                
                if (dist <= 15.0f) 
                {
                    return gw.groupAvailWeight; 
                }
            }
        }

        return 0f; 
    }
}