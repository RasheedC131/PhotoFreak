using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 

public class ScatterNpcs : MonoBehaviour
{
    private GuestSettings gs; 

    void Start()
    {
        gs = GuestSettings.Instance; 
        
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        if (allNodes.Length == 0) return; 

        AIContext[] allAgents = FindObjectsOfType<AIContext>();
        ShuffleArray(allAgents);

        int nodeIndex = 0;

        foreach (AIContext agent in allAgents)
        {
            if (agent != null && !agent.isMonster)
            {
                ZoneNode placementNode = allNodes[nodeIndex % allNodes.Length];
                nodeIndex++;

                NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
                
                if (navAgent != null)
                {
                    Vector3 offsetVec = new Vector3(Random.Range(-4.0f, 4.0f), 0, Random.Range(-4.0f, 4.0f));
                    Vector3 desiredPos = placementNode.transform.position + offsetVec;
                    
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPos, out hit, 10f, NavMesh.AllAreas))
                    {
                        navAgent.enabled = false;
                        agent.transform.position = hit.position;
                        navAgent.enabled = true;
                    }
                    else
                    {
                        navAgent.enabled = false;
                        agent.transform.position = placementNode.transform.position;
                        navAgent.enabled = true;
                    }

                    agent.currentDestination = placementNode.transform.position;

                    if (placementNode.HasOpenSlots())
                    {
                        placementNode.incomingCrowd.Add(agent);
                        agent.targetNode = placementNode.transform;
                        
                        ActionWanderNodes wanderScript = agent.GetComponent<ActionWanderNodes>();
                        if (wanderScript != null) wanderScript.hasReservedSpot = true;
                    }
                    else
                    {
                        agent.targetNode = null; 
                    }

                    agent.forceNewPath = false; 

                    navAgent.speed = Random.Range(gs.wanderBaseSpeed * 0.8f, gs.wanderBaseSpeed * 1.2f);
                    navAgent.acceleration = Random.Range(gs.wanderMinAcceleration, gs.wanderMaxAcceleration);
                    navAgent.avoidancePriority = Random.Range(30, 70);
                }
            }
        }
    }

    private void ShuffleArray(AIContext[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            AIContext temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}