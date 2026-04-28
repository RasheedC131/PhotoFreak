using UnityEngine;
using UnityEngine.AI;

public class ActionStalk : UtilityAction
{
    public AIContext currentStalker;
    private NavMeshAgent agent;
    private AIContext ctx;
    private MonsterSettings ms; 
    private AIBrain brain; 
    private NPCIdentity identity; 

    private float stalkBuffer = 1.5f;

    // Tracks which victim this monster has incremented the stalker count on.
    // Ensures AddStalker/RemoveStalker are always balanced even if the victim
    // changes or the action exits unexpectedly.
    private AIContext _registeredVictim = null;

    // Cache the last position we sent to SetDestination so we only recalculate
    // the path when the victim has actually moved a meaningful distance.
    // Without this every monster recalculates every brain tick while the victim
    // is standing still at the kill node, causing constant NavMesh churn.
    private Vector3 _lastDestination = Vector3.positiveInfinity;
    private const float DestMoveThreshold = 0.5f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        brain = GetComponentInParent<AIBrain>(); 
        identity = GetComponentInParent<NPCIdentity>(); 
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim == null || ctx.currentVictim.isMonster)
        {
            ctx.currentVictim = null;
            RegisterVictim(null);
            return;
        }

        // Keep the registration in sync if the victim ever changes mid-action.
        RegisterVictim(ctx.currentVictim);
        ctx.currentVictim.currentStalker = ctx;

        bool isFinalPanic       = CrowdStateManager.Instance != null && CrowdStateManager.Instance.IsFinalPanic;
        bool victimAtKillNode   = ctx.currentVictim.hasArrivedAtKillNode;

        // Conditions to switch over to attack
        //   Victim has reached the kill room, strike immediately once isolated
        //   Stalk timer has expired, strike once isolated
        //   Final panic is active, strike once isolated
        bool timerComplete   = ctx.currentStalkTimer >= ms.stalkDuration;
        bool isReadyToStrike = (victimAtKillNode || timerComplete || isFinalPanic) && IsAreaIsolated();

        if (isReadyToStrike)
        {
            if (identity != null) identity.ShowMonsterModel();

            agent.speed            = ms.revealedSpeed;
            agent.isStopped        = false;
            agent.stoppingDistance = 0.5f;
            SetDestinationIfMoved(ctx.currentVictim.transform.position);
            ctx.currentActionState = NPCActionState.WALK;
        }

        // stalk phase (follow the current victim )
        else
        {
            if (identity != null) identity.ShowGuestModel();
            agent.speed = ms.stalkSpeed;

            float currentDist  = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position);
            float tooCloseDist = ms.stalkDistance - 1.0f;
            float sweetSpotMax = ms.stalkDistance + stalkBuffer;

            if (currentDist < tooCloseDist)
            {
                // back off if the monster is too close 
                agent.isStopped        = false;
                agent.stoppingDistance = 0f;
                Vector3 dirAway    = (ctx.transform.position - ctx.currentVictim.transform.position).normalized;
                Vector3 retreatPos = ctx.transform.position + (dirAway * 2.0f);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(retreatPos, out hit, 2.0f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);

                ctx.currentActionState = NPCActionState.WALK;
            }
            else if (currentDist <= sweetSpotMax)
            {
                // try to maintain this spot away from the target guest 
                if (agent.isOnNavMesh) { agent.ResetPath(); agent.isStopped = true; }

                Vector3 lookPos = ctx.currentVictim.transform.position;
                lookPos.y = ctx.transform.position.y;
                Vector3 lookDir = lookPos - ctx.transform.position;
                if (lookDir.sqrMagnitude > 0.01f)
                    ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

                ctx.currentActionState = NPCActionState.IDLE;
            }
            else
            {
                // Move closer to the target 
                agent.isStopped        = false;
                agent.stoppingDistance = ms.stalkDistance;
                SetDestinationIfMoved(ctx.currentVictim.transform.position);
                ctx.currentActionState = NPCActionState.WALK;
            }
        }
        
        if (brain != null) ctx.currentStalkTimer += brain.decisionInterval; 
    }

    // kill logic, only kill if the room is cleared and the target has arrived at their node
    private bool IsAreaIsolated()
    {
        bool   victimAtKillNode = ctx.currentVictim != null && ctx.currentVictim.hasArrivedAtKillNode;
        Vector3 checkOrigin     = victimAtKillNode ? ctx.currentVictim.transform.position : ctx.transform.position;
        float checkRadius       = victimAtKillNode ? ms.killRoomWitnessRadius : ms.witnessRadius;

        Collider[] hits = Physics.OverlapSphere(checkOrigin, checkRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) return false;

            AIContext otherNPC = hit.GetComponentInParent<AIContext>();
            // Other monsters are co-conspirators, not witnesses — don't let them
            // block one monster from revealing and chasing its own victim.
            if (otherNPC != null && !otherNPC.isMonster && otherNPC != ctx && otherNPC != ctx.currentVictim)
                return false;
        }
        return true;
    }

    public override void OnExit()
    {
        RegisterVictim(null); // decrement victim's stalker count before leaving

        if (agent != null)
        {
            agent.speed            = ms != null ? ms.walkSpeed : 3.5f;
            agent.stoppingDistance = 0f;
            if (agent.isOnNavMesh) agent.isStopped = false;
        }

        if (ctx != null) ctx.currentActionState = NPCActionState.IDLE;
    }

    // Keeps _registeredVictim in sync with the actual victim and balances
    // the stalker reference count on both the old and new victim correctly.
    private void RegisterVictim(AIContext newVictim)
    {
        if (newVictim == _registeredVictim) return;

        if (_registeredVictim != null)
        {
            _registeredVictim.RemoveStalker();
            _lastDestination = Vector3.positiveInfinity;
        }

        _registeredVictim = newVictim;

        if (_registeredVictim != null)
            _registeredVictim.AddStalker();
    }

    // Only calls SetDestination when the victim has moved beyond DestMoveThreshold.
    // Prevents every monster from triggering a NavMesh recalculation every tick
    // while the victim is stationary at the kill node.
    private void SetDestinationIfMoved(Vector3 targetPos)
    {
        if (Vector3.Distance(targetPos, _lastDestination) < DestMoveThreshold) return;
        if (agent.isOnNavMesh) agent.SetDestination(targetPos);
        _lastDestination = targetPos;
    }
}