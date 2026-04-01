using UnityEngine;
using UnityEngine.AI;

public class ActionSocialize : UtilityAction
{
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private AIContext ctx;
    private GuestSettings gs; 
    
    [Header("Social Settings")]
    private bool hasJoinedGroup = false;
    private float joinHubRange; 
    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        obstacle = GetComponentInParent<NavMeshObstacle>();
        ctx = GetComponentInParent<AIContext>();
        gs = GuestSettings.Instance; 
        if (obstacle != null) obstacle.enabled = false;
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

        if (!hasJoinedGroup)
        {
            if (!agent.enabled)
            {
                if (obstacle != null) obstacle.enabled = false;
                agent.enabled = true;
            }

            agent.isStopped = false;
            agent.SetDestination(ctx.targetHub.transform.position);

            float dist = Vector3.Distance(transform.position, ctx.targetHub.transform.position);
            
            if (dist <= gs.socialArrivalDistance)
            {
                ctx.targetHub.IncomingAttendees = Mathf.Max(0, ctx.targetHub.IncomingAttendees - 1);
                ctx.targetHub.CurrentAttendees++;
                ctx.isOccupied = true;
                hasJoinedGroup = true;
                
                agent.enabled = false;
                if (obstacle != null) obstacle.enabled = true;
            } 
        }
        else
        {
            Vector3 lookPos = ctx.targetHub.transform.position;
            lookPos.y = transform.position.y; 
            
            Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * gs.socialTurnSpeed
            );
        }
    }

    private void FindClosestHub()
    {
        float closestDist = Mathf.Infinity;
        SocialHub bestHub = null;

        foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
        {
            if (hub != null && hub.HasOpenSlots())
            {
                float dist = Vector3.Distance(transform.position, hub.transform.position);
    
                if (dist < closestDist && dist <= joinHubRange)
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
        }
    }

    private void ResetSocialState()
    {
        if (ctx.targetHub != null && !hasJoinedGroup)
        {
            ctx.targetHub.IncomingAttendees = Mathf.Max(0, ctx.targetHub.IncomingAttendees - 1);
        }

        ctx.isOccupied = false;
        ctx.targetHub = null;
        hasJoinedGroup = false;
        ctx.targetNode = null;
        ctx.forceNewPath = true;

        if (obstacle != null) obstacle.enabled = false;
        agent.enabled = true;
    }
}