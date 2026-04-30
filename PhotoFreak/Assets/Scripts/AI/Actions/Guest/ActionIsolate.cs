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
    private float nodeArrivalTime = -1f;   
    private float _noStalkerSince = -1f;
    private const float NoStalkerGracePeriod = 2.0f;

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
    }

    void OnDestroy()
    {
        ReleaseCurrentNode();
        foreach (Transform node in killRoomNodes)
            AllKillNodes.Remove(node);
    }

    void Update()
    {
        if (ctx == null) return;

        if (!ctx.isBeingStalked)
        {
            bool hasArrived = nodeArrivalTime >= 0f;

            if (hasArrived)
            {
                // NPC is already waiting at the kill node — start the boredom clock.
                if ((Time.time - nodeArrivalTime) >= gs.killNodeBoredomTime)
                {
                    Debug.Log($"[{gameObject.name}] Kill node boredom exceeded — leaving node.");
                    _noStalkerSince = -1f;
                    ReleaseCurrentNode();
                }
            }
            else if (currentKillNode != null)
            {

                if (_noStalkerSince < 0f) _noStalkerSince = Time.time;

                if (Time.time - _noStalkerSince >= NoStalkerGracePeriod)
                {
                    Debug.Log($"[{gameObject.name}] Lost stalker en-route — releasing kill node.");
                    _noStalkerSince = -1f;
                    ReleaseCurrentNode();
                }
            }
            return;
        }

        // Stalker is present — reset the grace-period timer.
        _noStalkerSince = -1f;

        if (currentKillNode != null)
        {
            AIContext[] allNPCs = FindObjectsOfType<AIContext>();
            foreach (AIContext npc in allNPCs)
            {
                if (npc == ctx || npc.isMonster) continue;

                float distGuestToNode = Vector3.Distance(npc.transform.position, currentKillNode.position);
                if (distGuestToNode < gs.playerKillNodeAvoidRadius)
                {
                    ReleaseCurrentNode();
                    break;
                }
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
            
            if (!KillNodeRegistry.TryReserve(chosen, ctx)) return;

            currentKillNode = chosen;
            pathStartTime = Time.time;
            agent.ResetPath();
            agent.isStopped = false;
            ctx.currentDestination = currentKillNode.position;
            agent.SetDestination(ctx.currentDestination);
        }

        bool pathInvalid = !agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid;

        if (pathInvalid)
        {
            ReleaseCurrentNode();
            return;
        }

        // refresh the path so that the npc is committed 
        if (pathStartTime > 0f && (Time.time - pathStartTime) > gs.isolatePathfindingTimeout)
        {
            pathStartTime = Time.time;
            agent.isStopped = false;
            if (agent.isOnNavMesh) agent.SetDestination(currentKillNode.position);
        }

        // If npc has arrived at their target kill node
        float distToNode = Vector3.Distance(ctx.transform.position, currentKillNode.position);
        if (distToNode <= gs.isolateKillNodeArrivalDist)
        {
            agent.isStopped          = true;
            ctx.hasArrivedAtKillNode = true;

            // boredom time, leave if npc is waiting too long for the monster to arrive
            if (nodeArrivalTime < 0f) nodeArrivalTime = Time.time;
            transform.Rotate(0f, gs.isolateTurnAngle * Time.deltaTime, 0f);
        }
    }

    // Called when the utility brain switches away from this action
    public override void OnEnter()
    {
        // If we already have a reserved node (resumed from a previous run of
        // this action), refresh pathStartTime so the timeout doesn't
        // immediately fire on re-entry.
        if (currentKillNode != null)
        {
            pathStartTime = Time.time;
            agent.isStopped = false;
            if (agent.isOnNavMesh)
                agent.SetDestination(currentKillNode.position);
        }
    }

    public override void OnExit() { /* reservation kept intentionally — see OnEnter */ }

    // Called externally (e.g. from ActionAttack) to immediately free the slot
    // after the NPC has been infected so the next victim can claim it right away.
    public void ReleaseKillNode() => ReleaseCurrentNode();

    private void ReleaseCurrentNode()
    {
        if (currentKillNode != null)
        {
            KillNodeRegistry.Release(currentKillNode, ctx);
            currentKillNode = null;
        }

        pathStartTime   = -1f;
        nodeArrivalTime = -1f;
        _noStalkerSince = -1f;

        if (ctx != null)
            ctx.hasArrivedAtKillNode = false;
    }

    private Transform FindBestKillNode()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return null;

        Transform bestNode  = null;
        float     bestScore = float.NegativeInfinity;
        AIContext[] allNPCs = FindObjectsOfType<AIContext>();

        foreach (Transform node in killRoomNodes)
        {
            // tries to find the best possible node it can navigate to where the player won't be able to see them or any npcs 
            if (node == null) continue;

            if (KillNodeRegistry.IsReserved(node) && !KillNodeRegistry.IsReservedBy(node, ctx))
                continue;

            float closestGuestDist = float.MaxValue;
            foreach (AIContext npc in allNPCs)
            {
                if (npc == ctx || npc.isMonster) continue;
                float dist = Vector3.Distance(node.position, npc.transform.position);
                if (dist < closestGuestDist) closestGuestDist = dist;
            }

            if (closestGuestDist < gs.playerKillNodeAvoidRadius) continue;

            NavMeshPath path = new NavMeshPath();
            bool reachable   = agent.CalculatePath(node.position, path) && path.status != NavMeshPathStatus.PathInvalid;

            if (!reachable) continue;

            float distToSelf = Vector3.Distance(ctx.transform.position, node.position);
            float score      = closestGuestDist - distToSelf * 0.3f;

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

                if (KillNodeRegistry.IsReserved(node) && !KillNodeRegistry.IsReservedBy(node, ctx))
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