using UnityEngine;
using UnityEngine.AI; 

public class ConsiderationIsSolo : Consideration
{
    private AIContext ctx; 
    private GuestWeights gw; 
    private float personalityOffset; 
    private NavMeshAgent agent; 
    private ActionWanderNodes wanderNodesAction; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
        gw = GuestWeights.Instance; 
        agent = GetComponentInParent<NavMeshAgent>();
        wanderNodesAction = GetComponent<ActionWanderNodes>(); 
        
        if (gw != null) personalityOffset = Random.Range(-gw.maxPersonalityOffset, gw.maxPersonalityOffset);
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || ctx.isOccupied || ctx.isMonster || gw == null) return 0f;
        
        if (ctx.targetNode == null && ctx.targetHub == null && agent != null && agent.hasPath)
        {
            bool openNodesExist = false;
            if (wanderNodesAction != null)
            {
                openNodesExist = wanderNodesAction.HasOpenNodes();
            }

            bool openHubsExist = false;
            if (SocialHubManager.Instance != null)
            {
                foreach (SocialHub hub in SocialHubManager.Instance.activeHubs)
                {
                    if (hub != null && hub.HasOpenSlots())
                    {
                        openHubsExist = true;
                        break;
                    }
                }
            }

            if (openNodesExist || openHubsExist)
            {
                return Mathf.Clamp01(gw.soloWeight + personalityOffset);
            }

            return gw.soloComittedWeight; 
        }

        return Mathf.Clamp01(gw.soloWeight + personalityOffset);
    }
}