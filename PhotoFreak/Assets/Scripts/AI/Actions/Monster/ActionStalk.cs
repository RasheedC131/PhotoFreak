using UnityEngine;
using UnityEngine.AI;

public class ActionStalk : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private MonsterSettings ms; 
    private AIBrain brain; 
    private NPCIdentity identity; 

    // THE DEAD ZONE BUFFER
    // How much wiggle room the monster has before deciding to move again
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

        // PHASE 2: THE STRIKE (Timer has finished)
        if (ctx.currentStalkTimer >= ctx.stalkDuration)
        {
            // if (identity != null) identity.ShowMonsterModel(); 
            
            agent.isStopped = false;
            // Strike Mode: Zero out the stopping distance to get into attack range
            agent.stoppingDistance = 0.5f; 
            agent.SetDestination(ctx.currentVictim.transform.position); 
        }

        else
        {
            if (identity != null) identity.ShowGuestModel(); 
            
            float currentDist = Vector3.Distance(ctx.transform.position, ctx.currentVictim.transform.position);

            float tooCloseDist = ms.stalkDistance - 1.0f;
            float sweetSpotMax = ms.stalkDistance + stalkBuffer;

            // target moves too close move away 
            if (currentDist < tooCloseDist)
            {
                agent.isStopped = false;
                agent.stoppingDistance = 0f;
                Vector3 dirAwayFromPrey = (ctx.transform.position - ctx.currentVictim.transform.position).normalized;
                Vector3 retreatPos = ctx.transform.position + (dirAwayFromPrey * 2.0f);
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(retreatPos, out hit, 2.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            // stand still and observe if we are close enough
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
            // approach prey if we are too far away 
            else
            {
                agent.isStopped = false;
                agent.stoppingDistance = ms.stalkDistance;
                agent.SetDestination(ctx.currentVictim.transform.position);
            }
        }
        
        if (brain != null) ctx.currentStalkTimer += brain.decisionInterval; 
    }

    public override void OnExit()
    {
        if (agent != null) 
        {
            agent.stoppingDistance = 0f; 
            agent.isStopped = false; // Ensure they aren't frozen when switching actions
        }
    }
}