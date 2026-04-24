using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ActionIsolate : UtilityAction
{
    private NavMeshAgent  agent;
    private AIContext     ctx;
    private GuestSettings gs;

    [Header("Kill Room nodes")]
    public Transform killRoomNodesContainer;
    private List<Transform> killRoomNodes = new List<Transform>();
    public static readonly HashSet<Transform> AllKillNodes = new HashSet<Transform>();

    private Transform currentKillNode  = null;
    private bool isEnablingAgent = false;
    private float pathStartTime = -1f;
    private float nodeArrivalTime = -1f;   // set when hasArrivedAtKillNode first becomes true
    private Transform playerTransform;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx   = GetComponentInParent<AIContext>();
        gs    = GuestSettings.Instance;

        if (killRoomNodesContainer != null)
        {
            foreach (Transform child in killRoomNodesContainer)
            {
                killRoomNodes.Add(child);
                AllKillNodes.Add(child);   // register globally for wander avoidance
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] ActionIsolate: killRoomNodesContainer is not assigned.");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void OnDestroy()
    {
        foreach (Transform node in killRoomNodes)
            AllKillNodes.Remove(node);
    }

    void Update()
    {
        if (ctx == null) return;

        if (!ctx.isBeingStalked)
        {
            bool hasArrived = nodeArrivalTime >= 0f;
            bool isBored    = !hasArrived
                              || (Time.time - nodeArrivalTime) >= gs.killNodeBoredomTime;

            if (isBored)
            {
                if (hasArrived)
                    Debug.Log($"[{gameObject.name}] Kill node boredom exceeded — leaving node.");
                ReleaseCurrentNode();
            }
            return;
        }

        if (currentKillNode != null && playerTransform != null)
        {
            float distPlayerToNode = Vector3.Distance(playerTransform.position, currentKillNode.position);
            if (distPlayerToNode < gs.playerKillNodeAvoidRadius)
            {
                Debug.Log($"[{gameObject.name}] Player entered kill room — re-routing.");
                ReleaseCurrentNode();
            }
        }
    }

    public override void ExecuteAction()
    {
        if (killRoomNodes.Count == 0) return;

        if (!agent.enabled)
        {
            if (!isEnablingAgent) StartCoroutine(SafeEnableAgent());
            return;
        }

        if (!agent.isOnNavMesh) return;

        if (currentKillNode == null)
        {
            Transform chosen = FindBestKillNode();

            if (chosen == null) return;
            
            if (KillNodeRegistry.Instance != null && !KillNodeRegistry.Instance.TryReserve(chosen, ctx))
            {

                Debug.LogWarning($"[{gameObject.name}] Node {chosen.name} was claimed between find and reserve — retrying.");
                return;
            }

            currentKillNode = chosen;
            pathStartTime = Time.time;
            agent.ResetPath();
            agent.isStopped = false;
            ctx.currentDestination = currentKillNode.position;
            agent.SetDestination(ctx.currentDestination);

            Debug.Log($"[{gameObject.name}] Isolating → reserved and heading to {currentKillNode.name}");
        }

        bool pathInvalid = !agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid;
        bool timedOut = pathStartTime > 0f && (Time.time - pathStartTime) > gs.isolatePathfindingTimeout;

        if (pathInvalid || timedOut)
        {
            Debug.LogWarning($"[{gameObject.name}] Path to {currentKillNode.name} " +
                             $"failed ({(pathInvalid ? "invalid" : "timeout")}). Releasing and retrying.");
            ReleaseCurrentNode();
            return;
        }

        // arrived at node and we now need to wait on the monster
        if (!agent.pathPending && agent.hasPath && agent.remainingDistance <= gs.isolateKillNodeArrivalDist)
        {
            agent.isStopped          = true;
            ctx.hasArrivedAtKillNode = true;

            // boredom time, leave if npc is waiting too long for the monster to arrive
            if (nodeArrivalTime < 0f) nodeArrivalTime = Time.time;
            transform.Rotate(0f, gs.isolateTurnAngle * Time.deltaTime, 0f);
        }
    }


    public override void OnExit()
    {
        ReleaseCurrentNode();
    }

    private void ReleaseCurrentNode()
    {
        if (currentKillNode != null)
        {
            KillNodeRegistry.Instance?.Release(currentKillNode, ctx);
            currentKillNode = null;
        }

        pathStartTime   = -1f;
        nodeArrivalTime = -1f;

        if (ctx != null)
            ctx.hasArrivedAtKillNode = false;
    }


    private Transform FindBestKillNode()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return null;

        Transform bestNode  = null;
        float     bestScore = float.NegativeInfinity;

        foreach (Transform node in killRoomNodes)
        {

            // tries to find the best possible node it can navigate to where the player won't be able to see them or any npcs 
            if (node == null) continue;

            if (KillNodeRegistry.Instance != null
                && KillNodeRegistry.Instance.IsReserved(node)
                && !KillNodeRegistry.Instance.IsReservedBy(node, ctx))
                continue;

            float distToPlayer = playerTransform != null ? Vector3.Distance(node.position, playerTransform.position) : float.MaxValue;

            if (distToPlayer < gs.playerKillNodeAvoidRadius) continue;

            NavMeshPath path = new NavMeshPath();
            bool reachable   = agent.CalculatePath(node.position, path) && path.status != NavMeshPathStatus.PathInvalid;

            if (!reachable) continue;

            float distToSelf = Vector3.Distance(ctx.transform.position, node.position);
            float score      = distToPlayer - distToSelf * 0.3f;

            if (score > bestScore)
            {
                bestScore = score;
                bestNode  = node;
            }
        }

        // if we can't find a good node take what we can get 
        if (bestNode == null)
        {
            foreach (Transform node in killRoomNodes)
            {
                if (node == null) continue;

                if (KillNodeRegistry.Instance != null
                    && KillNodeRegistry.Instance.IsReserved(node)
                    && !KillNodeRegistry.Instance.IsReservedBy(node, ctx))
                    continue;

                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(node.position, path)
                    && path.status != NavMeshPathStatus.PathInvalid)
                {
                    bestNode = node;
                    break;
                }
            }
        }

        return bestNode;
    }

    private IEnumerator SafeEnableAgent()
    {
        isEnablingAgent = true;
        NavMeshObstacle obstacle = GetComponentInParent<NavMeshObstacle>();
        if (obstacle != null) obstacle.enabled = false;
        yield return null;
        agent.enabled    = true;
        isEnablingAgent = false;
    }
}
