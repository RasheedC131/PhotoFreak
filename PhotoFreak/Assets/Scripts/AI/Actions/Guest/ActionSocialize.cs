using UnityEngine;
using UnityEngine.AI;

public class ActionSocialize : UtilityAction
{
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private AIContext ctx;
    private GuestSettings gs; 
    private bool hasJoinedGroup = false;    
    private Vector3 assignedSpot; 

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        obstacle = GetComponentInParent<NavMeshObstacle>();
        ctx = GetComponentInParent<AIContext>();
        gs = GuestSettings.Instance; 
        if (obstacle != null) obstacle.enabled = false;
    }

    void Update()
    {
        if (hasJoinedGroup && ctx != null && ctx.targetHub != null)
        {
            Vector3 lookPos = ctx.targetHub.transform.position;
            lookPos.y = transform.position.y; 
            
            Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * gs.socialTurnSpeed);
        }
    }

    public override void ExecuteAction()
    {
        if (ctx.targetHub == null)
        {
            FindClosestHub();
            hasJoinedGroup = false;
        }

        if (ctx.targetHub == null)
        {
            ResetSocialState();
            return;
        }

        if (!hasJoinedGroup && !ctx.targetHub.HasOpenSlots())
        {
            LeaveGroup();
            return;
        }

        if (!hasJoinedGroup)
        {
            if (!agent.enabled)
            {
                if (obstacle != null) obstacle.enabled = false;
                agent.enabled = true;
            }

            if (agent.isOnNavMesh && agent.isStopped) agent.isStopped = false;

            if (agent.isOnNavMesh) agent.SetDestination(assignedSpot);

            float dist = Vector3.Distance(transform.position, assignedSpot);
            
            if (dist <= gs.socialArrivalDistance)
            {
                ctx.targetHub.IncomingAttendees = Mathf.Max(0, ctx.targetHub.IncomingAttendees - 1);
                ctx.targetHub.CurrentAttendees++;
                ctx.isOccupied = true;
                hasJoinedGroup = true;
                
                agent.enabled = false;
                if (obstacle != null) obstacle.enabled = true;
                // ctx.currentActionState = NPCActionState.SOCIALIZE;
            } 
        }
    }

    public void LeaveGroup()
    {
        if (hasJoinedGroup && ctx.targetHub != null)
        {
            ctx.targetHub.CurrentAttendees = Mathf.Max(0, ctx.targetHub.CurrentAttendees - 1);
        }
        ResetSocialState();
    }

    public override void OnExit()
    {
        LeaveGroup();
    }

    private void FindClosestHub()
    {
        float closestDist = Mathf.Infinity;
        SocialHub bestHub = null;

        if (SocialHubManager.Instance == null) return; 

        foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
        {
            if (hub != null && hub.HasOpenSlots())
            {
                float dist = Vector3.Distance(transform.position, hub.transform.position);
    
                if (dist < closestDist && dist <= gs.socialJoinHubRange)
                {
                    closestDist = dist;
                    bestHub = hub;
                }
            }
        }

        if (bestHub != null)
        {
            ctx.targetHub = bestHub;
            bestHub.IncomingAttendees++; 
            float randomAngle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle)) * gs.socialConvRadius;
            assignedSpot = bestHub.transform.position + offset;
        }
    }

    private void ResetSocialState()
    {
        if (ctx.targetHub != null && !hasJoinedGroup) ctx.targetHub.IncomingAttendees = Mathf.Max(0, ctx.targetHub.IncomingAttendees - 1);
        


        ctx.isOccupied = false;
        ctx.targetHub = null;
        hasJoinedGroup = false;
        ctx.targetNode = null;
        ctx.forceNewPath = true;

        ctx.currentActionState = NPCActionState.WALK; 

        if (obstacle != null) obstacle.enabled = false;
        if (agent != null) agent.enabled = true; 
    }
}