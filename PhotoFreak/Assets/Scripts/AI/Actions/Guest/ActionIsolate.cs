using UnityEngine;
using UnityEngine.AI; 
using System.Collections.Generic; 

public class ActionIsolate : UtilityAction
{
    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 
    private GuestSettings gs; 

    [Header("Kill Room nodes")]
    public Transform killRoomNodesContainer;
    private List<Transform> killRoomNodes = new List<Transform>(); 
    private Transform currentKillNode = null; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 
        gs = GuestSettings.Instance; 

        if (killRoomNodesContainer != null)
        {
            foreach(Transform child in killRoomNodesContainer)
            {
                killRoomNodes.Add(child); 
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} is missing the killRoomNodesContainer");
        }
    }

    void Update()
    {
        if (ctx != null && !ctx.isBeingStalked) currentKillNode = null;
    }

    public override void ExecuteAction()
    {
        if (killRoomNodes.Count == 0) return; 

        if (!agent.enabled)
        {
            NavMeshObstacle obstacle = GetComponentInParent<NavMeshObstacle>();
            if (obstacle != null) obstacle.enabled = false;
            agent.enabled = true;
        }

        if (!agent.isOnNavMesh) return; 

        if (currentKillNode == null) 
        {
            currentKillNode = killRoomNodes[Random.Range(0, killRoomNodes.Count)]; 
            agent.ResetPath(); 
            agent.isStopped = false; 
            ctx.currentDestination = currentKillNode.position; 
            agent.SetDestination(ctx.currentDestination); 
            
            Debug.Log($"Guest: {gameObject.name} is isolating! Heading to {currentKillNode.name}");
        }
        
        if (currentKillNode != null && !agent.pathPending && agent.remainingDistance <= gs.isolateKillNodeArrivalDist)        
        {
            agent.isStopped = true; 
            transform.Rotate(0, gs.isolateTurnAngle * Time.deltaTime, 0);   
        }
    }
}