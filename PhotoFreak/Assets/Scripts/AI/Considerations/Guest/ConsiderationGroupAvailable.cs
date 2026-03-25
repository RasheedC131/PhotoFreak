using UnityEngine;

public class Consideration_GroupAvailable : Consideration
{
    private AIContext context;

    void Awake()
    {
        context = GetComponentInParent<AIContext>();
    }

    protected override float EvaluateRawValue()
    {
        if (context.isOccupied || context.isMonster) return 0f;

        if (SocialHubManager.Instance == null || SocialHubManager.Instance.activeHubs.Count == 0) return 0f;

        foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
        {
            if (hub != null && hub.HasOpenSlots())
            {
                return 1.0f; // found an open hub 
            }
        }

        return 0f;      // couldn't find one 
    }
}