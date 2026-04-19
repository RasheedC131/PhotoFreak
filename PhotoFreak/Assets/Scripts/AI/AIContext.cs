using UnityEngine;
using UnityEngine.AI; 

public enum NPCActionState
{
    IDLE = 0, 
    WALK = 1, 
    DRINK = 2, 
    SOCIALIZE = 3
}
[SelectionBase]
public class AIContext : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent; 
    public Renderer myRenderer; 
    public Transform nodesContainer;       
    public Transform currentThreat; 

    [Header("Navigation Data")]
    public Transform[] zoneGroups; 
    public Transform currentZone; 
    public Transform targetNode; 
    public Vector3 currentDestination; 
    public bool forceNewPath = false; 
    public NPCActionState currentActionState = NPCActionState.IDLE;
    public float walkingStartTime = -1f;


    [Header("Social State")]
    public bool isOccupied = false; 
    public SocialHub targetHub; 
    public AIContext customLeader; 
    public int groupIdx = 0; 
    public int groupTotalSize = 1; 
    public bool isBeingStalked = false; 

    [Header("Monster State")]
    public bool isMonster = false; 
    public AIContext currentVictim; 
    public float currentStalkTimer; 
    public float stalkDuration = 60.0f; 
    public float stalkDistance = 10.0f; 

    private float MIN_WARP_OFFSET = -2.0f; 
    private float MAX_WARP_OFFSET = 2.0f; 

    // used for monster ai pathfinding aswell 
    protected virtual void Start()
    {
        if (agent is null) agent = GetComponent<NavMeshAgent>(); 

        if (agent is null)
        {
            Debug.LogError($"{gameObject.name}: Missing NavMeshAgent");
            return;
        }  

        SetupNavigation(); 
        agent.avoidancePriority = Random.Range(30, 70);         // prevents npcs from colliding into eachother by giving each npc a priority number

    }

    public void SetupNavigation(bool isMutating = false)
    {
        if (zoneGroups is not null && zoneGroups.Length > 0) return; 

        if (nodesContainer is null && SocialHubManager.Instance is not null)
        {
            nodesContainer = SocialHubManager.Instance.globalNodesContainer; 
        }

        if (nodesContainer is null) return; 

        int count = nodesContainer.childCount; 
        zoneGroups = new Transform[count]; 

        for (int i = 0; i < count; i++) 
        {
            zoneGroups[i] = nodesContainer.GetChild(i); 
        }
    }

    public float GetWalkingFatigue(float maxFatigueTime)
{
    if (walkingStartTime < 0f) return 0f;
    return Mathf.Clamp01((Time.time - walkingStartTime) / maxFatigueTime);
}
}
