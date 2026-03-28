using UnityEngine;

public class ActionIsolate : UtilityAction
{
    private UnityEngine.AI.NavMeshAgent agent; 
    private AIContext ctx; 

    [Header("Kill Room nodes")]
    public Transform [] killRoomNodes;

    public override void ExecuteAction()
    {
        if (killRoomNodes is null || killRoomNodes.Length == 0) return; 
        agent.isStopped = false; 
        
        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < 1.0f))
        {
            Transform chosenKillNode = killRoomNodes[Random.Range(0, killRoomNodes.Length)]; 

            ctx.currentDestination = chosenKillNode.position; 
            agent.SetDestination(ctx.currentDestination); 
            
            Debug.Log($"Guest {ctx.gameObject.name} is moving towards kill room");
        }
    } 
}
