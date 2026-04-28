using UnityEngine;

public class ConsiderationIsSolo : Consideration
{
    private AIContext ctx;
    private GuestWeights gw;
    private float personalityOffset;
    private UnityEngine.AI.NavMeshAgent agent;
    private ActionWanderNodes wanderNodesAction;

    [SerializeField] private float committedDecayMinTime = 5f;
    [SerializeField] private float committedDecayDuration = 10f;  
    [SerializeField] private AnimationCurve committedDecayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private float committedStartTime = -1f;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        gw = GuestWeights.Instance;
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        wanderNodesAction = GetComponent<ActionWanderNodes>();

        if (gw != null) personalityOffset = Random.Range(-gw.maxPersonalityOffset, gw.maxPersonalityOffset);
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || ctx.isOccupied || ctx.isMonster || gw == null) return 0f;

        // Once the NPC has realized it's being stalked, step aside so ActionIsolate can win.
        if (ctx.isAwareOfStalker) return 0f;

        if (ctx.targetNode == null && ctx.targetHub == null && agent != null && agent.hasPath)
        {
            bool openNodesExist = wanderNodesAction != null && wanderNodesAction.HasOpenNodes();

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
                return Mathf.Clamp01(gw.soloWeight + personalityOffset);

            if (committedStartTime < 0f) committedStartTime = Time.time;

            float timeCommitted = Time.time - committedStartTime;

            if (timeCommitted < committedDecayMinTime) return gw.soloComittedWeight;

            float decayProgress = Mathf.Clamp01((timeCommitted - committedDecayMinTime) / committedDecayDuration);
            float decayMultiplier = committedDecayCurve.Evaluate(decayProgress);
            return Mathf.Clamp01(gw.soloComittedWeight * decayMultiplier);
        }

        committedStartTime = -1f;
        return Mathf.Clamp01(gw.soloWeight + personalityOffset);
    }
}