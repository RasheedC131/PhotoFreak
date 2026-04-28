using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class ScatterNpcs : MonoBehaviour
{
    private GuestSettings gs; 

    private IEnumerator Start()
    {
        gs = GuestSettings.Instance; 
        
        yield return new WaitForEndOfFrame(); 

        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        if (allNodes.Length == 0) yield break; 

        AIContext[] allAgents = FindObjectsOfType<AIContext>();

        foreach (AIContext agent in allAgents)
        {
            if (agent != null && !agent.isMonster)
            {
                Transform placementNode = allNodes[Random.Range(0, allNodes.Length)].transform;
                Transform assignedNode = GetRandomOpenNode(allNodes);

                agent.targetNode = assignedNode; 
                agent.forceNewPath = true; 

                UnityEngine.AI.NavMeshAgent navAgent = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
                
                if (navAgent != null)
                {
                    Vector3 offsetVec = new Vector3(Random.Range(-4.0f, 4.0f), 0, Random.Range(-4.0f, 4.0f));
                    Vector3 targetPos = placementNode.position + offsetVec;
                    navAgent.enabled = false;
                    agent.transform.position = targetPos;
                    navAgent.enabled = true;

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