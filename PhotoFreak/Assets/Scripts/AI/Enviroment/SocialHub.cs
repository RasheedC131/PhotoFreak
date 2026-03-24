using UnityEngine;

// used to create temporary areas where the npcs can travel to 
public class SocialHub : MonoBehaviour
{
    public int MaxCapacity { get; private set; }
    public int CurrentAttendees { get; set; } = 0;
    
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
        if (SocialDirector.Instance != null)
        {
            SocialDirector.Instance.activeHubs.Remove(this);
        }
    }

    public bool HasOpenSlots()
    {
        return CurrentAttendees < MaxCapacity;
    }
}