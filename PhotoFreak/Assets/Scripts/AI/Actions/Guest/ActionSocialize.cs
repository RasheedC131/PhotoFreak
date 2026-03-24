using UnityEngine;
using UnityEngine.AI;

// handles navigation to the social hub where other agents wander to 
public class ActionSocialize : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext context;
    
    [Header("Social Settings")]
    [SerializeField] private float arrivalDistance = 2.0f;
    private bool hasJoinedGroup = false;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        context = GetComponentInParent<AIContext>();
    }

    public override void ExecuteAction()
    {
        if (context.targetHub == null)
        {
            FindClosestHub();
            hasJoinedGroup = false;
        }

        if (context.targetHub == null)
        {
            ResetSocialState();
            return;
        }

        if (!hasJoinedGroup)
        {
            agent.isStopped = false;
            agent.SetDestination(context.targetHub.transform.position);

            float dist = Vector3.Distance(transform.position, context.targetHub.transform.position);
            
            if (dist <= arrivalDistance)
            {
                if (context.targetHub.HasOpenSlots())
                {
                    context.targetHub.CurrentAttendees++;
                    context.isOccupied = true;
                    hasJoinedGroup = true;
                    
                    agent.isStopped = true; 
                }
                else 
                {
                    context.targetHub = null;
                }
            }
        }
        else
        {
            Vector3 lookPos = context.targetHub.transform.position;
            lookPos.y = transform.position.y; 
            
            Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
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

        context.targetHub = bestHub;
    }

    private void ResetSocialState()
    {
        context.isOccupied = false;
        context.targetHub = null;
        hasJoinedGroup = false;
        agent.isStopped = false;
    }
}