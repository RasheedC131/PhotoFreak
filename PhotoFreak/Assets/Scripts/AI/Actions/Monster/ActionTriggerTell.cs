using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations.Rigging; 

public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private MonsterSettings ms; 
    private ConsiderationTellCooldown tellTimer; 

    private bool _isPerformingTell = false; 
    public bool isPerformingTell => _isPerformingTell; 

    [Header("IK Rigging Dependencies")]
    [Tooltip("Drag the Multi-Aim Constraint from the Rig here.")]
    public MultiAimConstraint headSnapConstraint; 
    [Tooltip("The target the head will snap to (e.g., the player or a point in space).")]
    public Transform headSnapTarget; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        tellTimer = GetComponent<ConsiderationTellCooldown>(); 
        if (tellTimer == null) Debug.LogError("ConsiderationTellCooldown not found.");
    }

    public override void ExecuteAction()
    {
        if (!isPerformingTell)
        {
            StartCoroutine(PerformIKTellRoutine()); 
        }
    }

    private IEnumerator PerformIKTellRoutine()
    {
        _isPerformingTell = true; 
        tellTimer.ResetTimer(); 

        // Optional: Stop the agent to make the tell more pronounced
        ctx.agent.isStopped = true; 

        // Choose a random tell to perform if you have multiple
        yield return StartCoroutine(HeadSnapRoutine());
        
        ctx.agent.isStopped = false; 
        _isPerformingTell = false; 
    }

    private IEnumerator HeadSnapRoutine()
    {
        if (headSnapConstraint == null || headSnapTarget == null) yield break;

        // Position the target (e.g., directly at the current victim if one exists, or just forward)
        if (ctx.currentVictim != null)
        {
            headSnapTarget.position = ctx.currentVictim.transform.position + Vector3.up * 1.5f; 
        }
        else
        {
            headSnapTarget.position = ctx.transform.position + ctx.transform.forward * 5f + Vector3.up * 1.5f;
        }

        // 1. The Snap In (Lerp weight from 0 to 1 very quickly)
        float elapsedTime = 0f;
        float snapSpeed = 0.1f; 

        while (elapsedTime < snapSpeed)
        {
            elapsedTime += Time.deltaTime;
            headSnapConstraint.weight = Mathf.Lerp(0f, 1f, elapsedTime / snapSpeed);
            yield return null;
        }
        headSnapConstraint.weight = 1f;

        // 2. Hold the stare for the duration of the tell
        yield return new WaitForSeconds(ms.tellDuration);

        // 3. The Release (Lerp weight back to 0 to return to normal animation)
        elapsedTime = 0f;
        float releaseSpeed = 0.3f; 

        while (elapsedTime < releaseSpeed)
        {
            elapsedTime += Time.deltaTime;
            headSnapConstraint.weight = Mathf.Lerp(1f, 0f, elapsedTime / releaseSpeed);
            yield return null;
        }
        headSnapConstraint.weight = 0f;
    }

    public override void OnExit()
    {
        StopAllCoroutines();
        
        // Safety Reset: Ensure IK constraints are disabled if the action is interrupted
        if (headSnapConstraint != null) headSnapConstraint.weight = 0f;
        
        ctx.agent.isStopped = false;
        _isPerformingTell = false;
    }
}