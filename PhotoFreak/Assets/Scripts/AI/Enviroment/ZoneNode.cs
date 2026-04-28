using UnityEngine;

// npcs hang around these nodes 
public class ZoneNode : MonoBehaviour
{
    private static AIContext[] allAgents; 
    private GuestSettings gs; 
    
    public int activeCapacity { get; private set; } 

    void Awake()
    {
        gs = GuestSettings.Instance; 
        activeCapacity = Random.Range(2, gs.wanderNodeMaxCapacity + 1);

        if (allAgents == null) allAgents = FindObjectsOfType<AIContext>();
    }

    public int GetCurrentCrowd()
    {
        int currentCrowd = 0;
        if (allAgents == null) allAgents = FindObjectsOfType<AIContext>(); 
        
        foreach (AIContext agent in allAgents)
        {
            if (agent != null && !agent.isMonster && agent.targetNode == this.transform)
            {
                currentCrowd++;
            }
        }
        return currentCrowd;
    }
}