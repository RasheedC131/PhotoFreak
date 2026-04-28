using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ScatterNpcs : MonoBehaviour
{
    private GuestSettings gs;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gs = GuestSettings.Instance;
        ScatterAll();
    }

    private void ScatterAll()
    {
        ZoneNode[] allNodes = FindObjectsOfType<ZoneNode>();
        if (allNodes.Length == 0) return;

        // Clear any stale crowd data left over from the previous session so
        foreach (ZoneNode node in allNodes)
        {
            node.incomingCrowd.Clear();
            node.currentCrowd.Clear();
        }

        AIContext[] allAgents = FindObjectsOfType<AIContext>();
        ShuffleArray(allAgents);

        int nodeIndex = 0;

        foreach (AIContext agent in allAgents)
        {
            if (agent == null) continue;

            ZoneNode placementNode = allNodes[nodeIndex % allNodes.Length];
            nodeIndex++;

            NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
            if (navAgent == null) continue;

            // Ensure the agent component is in a clean state before teleporting.

            NavMeshObstacle obstacle = agent.GetComponent<NavMeshObstacle>();
            if (obstacle != null) obstacle.enabled = false;

            Vector3 offsetVec   = new Vector3(Random.Range(-4.0f, 4.0f), 0, Random.Range(-4.0f, 4.0f));
            Vector3 desiredPos  = placementNode.transform.position + offsetVec;

            navAgent.enabled = false;

            NavMeshHit hit;
            agent.transform.position = NavMesh.SamplePosition(desiredPos, out hit, 10f, NavMesh.AllAreas)
                ? hit.position
                : placementNode.transform.position;

            navAgent.enabled = true;

            agent.currentDestination = placementNode.transform.position;

            // Reset AI state so the brain starts fresh.
            agent.targetNode         = null;
            agent.forceNewPath       = false;
            agent.isAwareOfStalker   = false;
            agent.hasArrivedAtKillNode = false;

            if (placementNode.HasOpenSlots())
            {
                placementNode.incomingCrowd.Add(agent);
                agent.targetNode = placementNode.transform;

                // ActionWanderNodes lives on a child GameObject, not the root.
                ActionWanderNodes wanderScript = agent.GetComponentInChildren<ActionWanderNodes>(true);
                if (wanderScript != null) wanderScript.hasReservedSpot = true;
            }

            if (gs != null)
            {
                navAgent.speed            = Random.Range(gs.wanderBaseSpeed * 0.8f, gs.wanderBaseSpeed * 1.2f);
                navAgent.acceleration     = Random.Range(gs.wanderMinAcceleration, gs.wanderMaxAcceleration);
                navAgent.avoidancePriority = Random.Range(30, 70);
            }
        }
    }

    private void ShuffleArray(AIContext[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex  = Random.Range(i, array.Length);
            AIContext temp   = array[i];
            array[i]         = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
