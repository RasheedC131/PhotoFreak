using UnityEngine;
using UnityEngine.AI; 
using System.Collections.Generic; 

public class ActionIsolate : UtilityAction
{
    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 

    [Header("Kill Room nodes")]
    public Transform killRoomNodesContainer;
    private List<Transform> killRoomNodes = new List<Transform>(); 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 

        if (killRoomNodesContainer is not null)
        {
            foreach(Transform child in killRoomNodesContainer)
            {
                killRoomNodes.Add(child); 
            }

            return; 
        }

        Debug.LogWarning($"{gameObject.name} is missing the killRoomNodesContainer");
    }

    public override void ExecuteAction()
    {
        if (killRoomNodes is null || killRoomNodes.Count == 0) return; 
        agent.isStopped = false; 
        
        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < 1.0f))
        {
            Transform chosenKillNode = killRoomNodes[Random.Range(0, killRoomNodes.Count)]; 

            ctx.currentDestination = chosenKillNode.position; 
            agent.SetDestination(ctx.currentDestination); 
            
            Debug.Log($"Guest {ctx.gameObject.name} is moving towards kill room");
        }
    } 
}
