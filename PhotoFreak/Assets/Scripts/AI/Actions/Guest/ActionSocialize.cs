using UnityEngine;
using UnityEngine.AI;

// handles navigation to the social hub where other agents wander to 
public class ActionSocialize : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private GuestSettings gs; 
    
    [Header("Social Settings")]
    private bool hasJoinedGroup = false;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        gs = GuestSettings.Instance; 
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
            agent.isStopped = false;
            agent.SetDestination(ctx.targetHub.transform.position);

            float dist = Vector3.Distance(transform.position, ctx.targetHub.transform.position);
            
            if (dist <= gs.socialArrivalDistance)
            {
                if (ctx.targetHub.HasOpenSlots())
                {
                    ctx.targetHub.CurrentAttendees++;
                    ctx.isOccupied = true;
                    hasJoinedGroup = true;
                    
                    agent.isStopped = true; 
                }
                else 
                {
                    ctx.targetHub = null;
                }
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
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestHub = hub;
                }
            }
        }

        ctx.targetHub = bestHub;
    }

    private void ResetSocialState()
    {
        ctx.isOccupied = false;
        ctx.targetHub = null;
        hasJoinedGroup = false;
        agent.isStopped = false;
    }
}