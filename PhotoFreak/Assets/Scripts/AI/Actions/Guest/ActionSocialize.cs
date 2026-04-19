using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ActionSocialize : UtilityAction
{
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private AIContext ctx;
    private GuestSettings gs; 
    
    public Transform currentPartner; 
    public bool isHost = false; 
    private bool isSwitchingToAgent = false;

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
        if (ctx == null) return;

        if (ctx.currentActionState == NPCActionState.SOCIALIZE && currentPartner != null)
            FacePos(currentPartner.position, 6f);
        else if (ctx.currentActionState == NPCActionState.IDLE && ctx.targetNode != null && currentPartner == null)
            FacePos(ctx.targetNode.position, 3f);
        else if (ctx.currentActionState == NPCActionState.IDLE && currentPartner != null && isHost)
            FacePos(currentPartner.position, 3f);
    }

    // have them look at each other 
    private void FacePos(Vector3 lookPos, float speed)
    {
        lookPos.y = ctx.transform.position.y;
        Vector3 dir = lookPos - ctx.transform.position;
        if (dir.sqrMagnitude > 0.01f) ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * speed);
    }

    public override void ExecuteAction()
    {
        if (ctx == null || agent == null || ctx.targetNode == null) return;

        ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
        if (nodeScript == null || !nodeScript.currentCrowd.Contains(ctx)) return;

        if (currentPartner != null)
        {
            AIContext pCtx = currentPartner.GetComponent<AIContext>();
            if (pCtx == null || pCtx.targetNode != ctx.targetNode) BreakPartnerLink(); 
        }

        if (currentPartner == null) currentPartner = FindPartner(nodeScript);

        // socialize logic 
        if (currentPartner != null)
        {
            float dist = Vector3.Distance(ctx.transform.position, currentPartner.position);
            bool closeEnough = (dist <= 2.2f); 

            if (!closeEnough && dist <= 3.5f)
            {
                ActionSocialize pSocialize = currentPartner.GetComponentInChildren<ActionSocialize>();                
                if (pSocialize != null && pSocialize.ctx.currentActionState == NPCActionState.SOCIALIZE) 
                    closeEnough = true;
                else if (agent.enabled && agent.velocity.sqrMagnitude < 0.2f) 
                    closeEnough = true;
                else if (pSocialize != null && pSocialize.agent != null && pSocialize.agent.enabled && pSocialize.agent.velocity.sqrMagnitude < 0.2f) 
                    closeEnough = true;
            }
            
            if (!closeEnough) 
            {
                if (isHost)
                {
                    if (agent.enabled && !isSwitchingToAgent) ctx.StartCoroutine(SafeDisableAgent());
                    ctx.currentActionState = NPCActionState.IDLE;
                }
                else
                {
                    if (!agent.enabled && !isSwitchingToAgent) ctx.StartCoroutine(SafeEnableAgent());
                    
                    if (agent.enabled && !isSwitchingToAgent && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        Vector3 dirFromHost = (ctx.transform.position - currentPartner.position).normalized;
                        if (dirFromHost.sqrMagnitude < 0.01f) dirFromHost = ctx.transform.forward; 
                        Vector3 approachPos = currentPartner.position + (dirFromHost * 1.5f);
                        
                        if (!agent.hasPath || Vector3.Distance(agent.destination, approachPos) > 0.5f) agent.SetDestination(approachPos);
                        ctx.currentActionState = NPCActionState.WALK;
                    }
                }
            }
            else 
            {
                if (agent.enabled && !isSwitchingToAgent) ctx.StartCoroutine(SafeDisableAgent());
                ctx.currentActionState = NPCActionState.SOCIALIZE; 
            }
        }
        else 
        {
            if (agent.enabled && !isSwitchingToAgent) ctx.StartCoroutine(SafeDisableAgent());
            ctx.currentActionState = NPCActionState.IDLE;
        }
    }

    private IEnumerator SafeDisableAgent()
    {
        isSwitchingToAgent = true;
        if (agent != null)
        {
            if (agent.isOnNavMesh) { agent.ResetPath(); agent.velocity = Vector3.zero; }
            agent.enabled = false;
        }
        yield return null; 
        if (obstacle != null) obstacle.enabled = true;
        isSwitchingToAgent = false;
    }

    private IEnumerator SafeEnableAgent()
    {
        isSwitchingToAgent = true;
        if (obstacle != null) obstacle.enabled = false;
        yield return null; 
        if (agent != null) 
        {
            agent.enabled = true;
            if (agent.isOnNavMesh) agent.ResetPath();
        }
        isSwitchingToAgent = false;
    }

    private Transform FindPartner(ZoneNode node)
    {
        float closestDist = Mathf.Infinity;
        Transform bestPartner = null;

        foreach (AIContext otherCtx in node.currentCrowd)
        {
            if (otherCtx != null && otherCtx != ctx && !otherCtx.isMonster)
            {
                ActionSocialize otherSocial = otherCtx.GetComponentInChildren<ActionSocialize>();
                if (otherSocial != null && (otherSocial.currentPartner == null || otherSocial.currentPartner == ctx.transform))
                {
                    float dist = Vector3.Distance(ctx.transform.position, otherCtx.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestPartner = otherCtx.transform;
                    }
                }
            }
        }

        if (bestPartner != null)
        {
            ActionSocialize partnerSocial = bestPartner.GetComponentInChildren<ActionSocialize>();
            if (partnerSocial != null && partnerSocial.currentPartner == null)
            {
                partnerSocial.currentPartner = ctx.transform; 
                partnerSocial.isHost = true; 
                this.isHost = false; 
            }
        }
        return bestPartner;
    }

    private void BreakPartnerLink()
    {
        if (currentPartner != null)
        {
            ActionSocialize pSocial = currentPartner.GetComponentInChildren<ActionSocialize>();
            if (pSocial != null && pSocial.currentPartner == ctx.transform)
            {
                pSocial.currentPartner = null;
                pSocial.isHost = false; 
            }
        }
        currentPartner = null;
    }

    public void LeaveGroup()
    {
        if (ctx.targetNode != null)
        {
            ZoneNode nodeScript = ctx.targetNode.GetComponent<ZoneNode>();
            if (nodeScript != null) nodeScript.currentCrowd.Remove(ctx);
        }

        BreakPartnerLink();
        ctx.targetNode = null;
        ctx.forceNewPath = true; 
        isHost = false; 
        ctx.currentActionState = NPCActionState.WALK;
        
        if (!agent.enabled && !isSwitchingToAgent && gameObject.activeInHierarchy) ctx.StartCoroutine(SafeEnableAgent());
    }

    public override void OnExit()
    {
        LeaveGroup(); 
    }
}