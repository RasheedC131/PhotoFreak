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
    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>(); 
        ctx = GetComponentInParent<AIContext>(); 
        gs = GuestSettings.Instance; 

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
        if (killRoomNodes.Count == 0) return; 

        if (!ctx.isBeingStalked) 
        {
            Transform chosenKillNode = killRoomNodes[Random.Range(0, killRoomNodes.Count)]; 
            agent.isStopped = false; 
            ctx.currentDestination = chosenKillNode.position; 
            agent.SetDestination(ctx.currentDestination); 
        }
        
        if (ctx.isBeingStalked && !agent.pathPending && agent.remainingDistance < gs.isolateKillNodeArrivalDist)        
        {
            agent.isStopped = true; 
            // TODO: maybe rotate them with the rig 
            transform.Rotate(0, gs.isolateTurnAngle * Time.deltaTime, 0);   
            Debug.Log($"Guest: {gameObject.name} arrived at kill node"); 
        }
    } 
}
