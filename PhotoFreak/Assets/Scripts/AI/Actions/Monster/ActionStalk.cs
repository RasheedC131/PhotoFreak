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
            return;
        }

        ctx.currentVictim.isBeingStalked = true; 
        ctx.currentVictim.currentStalker = ctx;

        bool isReadyToStrike = ctx.currentStalkTimer >= ms.stalkDuration && IsAreaIsolated();

        // strike phase 
        if (isReadyToStrike)
        {
            if (identity != null) identity.ShowMonsterModel(); 
            
            agent.isStopped = false;
            agent.stoppingDistance = 0.5f; 
            agent.SetDestination(ctx.currentVictim.transform.position); 
        }

        // stalk phase
        else
        {
            // Stay disguised and patiently follow them to the Kill Room
            if (identity != null) identity.ShowGuestModel(); 
            
            float currentDist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position);

            float tooCloseDist = ms.stalkDistance - 1.0f;
            float sweetSpotMax = ms.stalkDistance + stalkBuffer;

            if (currentDist < tooCloseDist)
            {
                agent.isStopped = false;
                agent.stoppingDistance = 0f;
                Vector3 dirAwayFromPrey = (ctx.transform.position - ctx.currentVictim.transform.position).normalized;
                Vector3 retreatPos = ctx.transform.position + (dirAwayFromPrey * 2.0f);
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(retreatPos, out hit, 2.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            }
            else if (currentDist >= tooCloseDist && currentDist <= sweetSpotMax)
            {
                if (agent.isOnNavMesh)
                {
                    agent.ResetPath(); 
                    agent.isStopped = true;
                }

                Vector3 lookPos = ctx.currentVictim.transform.position;
                lookPos.y = ctx.transform.position.y;
                Vector3 lookDir = lookPos - ctx.transform.position;
                
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
                }
            }
            else
            {
                agent.isStopped = false;
                agent.stoppingDistance = ms.stalkDistance;
                agent.SetDestination(ctx.currentVictim.transform.position);
            }
        }
        
        if (brain != null) ctx.currentStalkTimer += brain.decisionInterval; 
    }

    private bool IsAreaIsolated()
    {
        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, ms.witnessRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) return false; 

            AIContext otherNPC = hit.GetComponentInParent<AIContext>();
            if (otherNPC != null && otherNPC != ctx && otherNPC != ctx.currentVictim)
            {
                return false; 
            }
        }
        return true;
    }

    public override void OnExit()
    {
        if (agent != null) 
        {
            agent.stoppingDistance = 0f; 
            if (agent.isOnNavMesh) agent.isStopped = false; 
        }
    }
}