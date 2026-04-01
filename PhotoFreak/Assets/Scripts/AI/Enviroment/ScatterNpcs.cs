using UnityEngine;
using System.Collections.Generic; 

// randomizes the placement of npcs on startup 
public class CrowdScatterer : MonoBehaviour
{
    GuestSettings gs; 
    void Start()
    {
        gs = GuestSettings.Instance; 
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        if (allNodes.Length == 0) return;
        AIContext[] allAgents = FindObjectsOfType<AIContext>();

        foreach (AIContext agent in allAgents)
        {
            if (agent != null && !agent.isMonster)
            {
                agent.targetNode = GetRandomOpenNode(allNodes);
                agent.forceNewPath = true; 

                UnityEngine.AI.NavMeshAgent navAgent = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navAgent != null)
                {
                    float baseSpeed = gs.wanderBaseSpeed * 0.5f; 
                    navAgent.speed = Random.Range(gs.wanderBaseSpeed * 0.8f, gs.wanderBaseSpeed * 1.2f);
                    navAgent.acceleration = Random.Range(gs.wanderMinAcceleration, gs.wanderMaxAcceleration);
                    navAgent.avoidancePriority = Random.Range(30, 70);
                }
            }
        }
        
        Debug.Log($"Scattered {allAgents.Length} agents across {allNodes.Length} nodes.");
    }

    private Transform GetRandomOpenNode(ZoneNode[] nodes)
    {
        List<ZoneNode> shuffledNodes = new List<ZoneNode>(nodes);
        for (int i = 0; i < shuffledNodes.Count; i++)
        {
            ZoneNode temp = shuffledNodes[i];
            int randomIndex = Random.Range(i, shuffledNodes.Count);
            shuffledNodes[i] = shuffledNodes[randomIndex];
            shuffledNodes[randomIndex] = temp;
        }

        foreach (ZoneNode node in shuffledNodes)
        {
            if (node.GetCurrentCrowd() < node.activeCapacity) return node.transform;
        }
        return null; 
    }
}