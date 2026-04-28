using UnityEngine;
using System.Collections.Generic;

// used to create temporary areas where the npcs can travel to
public class SocialHub : MonoBehaviour
{
    public int MaxCapacity { get; private set; }
    
    public List<AIContext> currentAttendees = new List<AIContext>();
    public List<AIContext> incomingAttendees = new List<AIContext>();
    
    public void Initialize(int capacity, float timeToLive)
    {
        MaxCapacity = capacity;
        Invoke(nameof(DisbandHub), timeToLive);
    }

    private void DisbandHub()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (SocialHubManager.Instance != null)
        {
            SocialHubManager.Instance.activeHubs.Remove(this);
        }
    }

    public bool HasOpenSlots()
    {
        return (currentAttendees.Count + incomingAttendees.Count) < MaxCapacity;    
    }
}