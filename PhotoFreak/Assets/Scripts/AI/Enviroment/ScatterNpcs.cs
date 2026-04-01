using UnityEngine;

public class CrowdScatterer : MonoBehaviour
{
    void Start()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        if (allNodes.Length == 0) return;
        AIContext[] allAgents = FindObjectsOfType<AIContext>();

        foreach (AIContext agent in allAgents)
        {
            if (agent != null && !agent.isMonster)
            {
                int randomIdx = Random.Range(0, allNodes.Length);
                agent.targetNode = allNodes[randomIdx].transform;
                agent.forceNewPath = true; 
            }
        }
        
        Debug.Log($"Scattered {allAgents.Length} agents across {allNodes.Length} nodes.");
    }
}