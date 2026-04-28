using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations.Rigging; 

public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private NavMeshAgent agent; 
    private MonsterSettings ms; 
    private ConsiderationTellCooldown tellTimer; 
    private NPCIdentity identity;

    private bool _isPerformingTell = false; 
    public bool isPerformingTell => _isPerformingTell; 

    [Header("IK Rigging Dependencies")]
    public MultiAimConstraint headSnapConstraint; 
    public Transform headSnapTarget; 
    
    public TwoBoneIKConstraint legIKConstraint;
    public Transform legIKHint;
    private Vector3 originalHintLocalPos;

    [Header("Procedural Tells (IK)")]
    public OverrideTransform proceduralLimpConstraint; 
    public OverrideTransform proceduralSlouchConstraint; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        agent = GetComponentInParent<NavMeshAgent>();
        ms = MonsterSettings.Instance; 
        tellTimer = GetComponent<ConsiderationTellCooldown>(); 
        identity = GetComponentInParent<NPCIdentity>(); // Cache identity

        if (legIKHint != null) originalHintLocalPos = legIKHint.localPosition;
    }

    public override void ExecuteAction()
    {
        // THE DISGUISE CHECK: If the monster model is showing, abort the tell!
        if (identity != null && !identity.isDisguised) return;
        

        if (!isPerformingTell)
        {
            Debug.Log($"[{ctx.gameObject.name}] Initiating Monster Tell...");
            StartCoroutine(PerformRandomTellRoutine()); 
        }
    }

    private IEnumerator PerformRandomTellRoutine()
    {
        _isPerformingTell = true; 
        tellTimer.ResetTimer(); 

        int tellChoice =  Random.Range(0, 4);

        switch (tellChoice)
        {
            case 0: yield return StartCoroutine(HeadSnapRoutine()); break;
            case 1: yield return StartCoroutine(HyperextendJointRoutine()); break;
            case 2: yield return StartCoroutine(HeavyLimpRoutine()); break;
            case 3: yield return StartCoroutine(SlouchRoutine()); break;
        }
        
        _isPerformingTell = false; 
    }

    // --- TELL 1: THE ROOM SCAN ---
    private IEnumerator HeadSnapRoutine()
    {
        if (headSnapConstraint == null || headSnapTarget == null) yield break;

        if (agent != null) agent.isStopped = true;

        Vector3 baseForward = ctx.transform.forward;
        if (ctx.currentVictim != null)
        {
            Vector3 dirToVictim = (ctx.currentVictim.transform.position - ctx.transform.position).normalized;
            dirToVictim.y = 0; 
            baseForward = dirToVictim.normalized;
        }

        Vector3 basePos = ctx.transform.position + (baseForward * 5f) + (Vector3.up * 1.5f);
        headSnapTarget.position = basePos;

        float t = 0f;
        while (t < 0.2f) 
        {
            t += Time.deltaTime;
            headSnapConstraint.weight = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        headSnapConstraint.weight = 1f;

        float scanTimer = 0f;
        Vector3 scanRightVector = Vector3.Cross(Vector3.up, baseForward).normalized;

        while (scanTimer < ms.tellDuration)
        {
            scanTimer += Time.deltaTime;
            Vector3 sweepOffset = scanRightVector * (Mathf.Sin(scanTimer * 5f) * 4f);
            headSnapTarget.position = basePos + sweepOffset;
            yield return null;
        }

        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            headSnapConstraint.weight = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
        headSnapConstraint.weight = 0f;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    // --- TELL 2: JOINT HYPEREXTENSION (Bird Leg) ---
    private IEnumerator HyperextendJointRoutine()
    {
        if (legIKConstraint == null || legIKHint == null) yield break;

        legIKHint.localPosition = new Vector3(originalHintLocalPos.x, originalHintLocalPos.y, -originalHintLocalPos.z);
        legIKConstraint.weight = 1f;

        yield return new WaitForSeconds(ms.tellDuration);

        legIKConstraint.weight = 0f;
        legIKHint.localPosition = originalHintLocalPos;
    }

    // --- TELL 3: PROCEDURAL LIMP (The Hip Drop) ---
    private IEnumerator HeavyLimpRoutine()
    {
        if (proceduralLimpConstraint == null) yield break; 

        float t = 0f;
        float transitionTime = 0.5f;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            proceduralLimpConstraint.weight = Mathf.Lerp(0f, 1f, t / transitionTime);
            yield return null;
        }
        proceduralLimpConstraint.weight = 1f;
        
        yield return new WaitForSeconds(ms.tellDuration);

        t = 0f;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            proceduralLimpConstraint.weight = Mathf.Lerp(1f, 0f, t / transitionTime);
            yield return null;
        }
        proceduralLimpConstraint.weight = 0f;
    }

    // --- TELL 4: PROCEDURAL SLOUCH (IK Upper Spine Bend) ---
    private IEnumerator SlouchRoutine()
    {
        if (proceduralSlouchConstraint == null) yield break; 

        if (agent != null) agent.isStopped = true;

        float t = 0f;
        float transitionTime = 0.5f;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            proceduralSlouchConstraint.weight = Mathf.Lerp(0f, 1f, t / transitionTime);
            yield return null;
        }
        proceduralSlouchConstraint.weight = 1f;

        yield return new WaitForSeconds(ms.tellDuration);

        t = 0f;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            proceduralSlouchConstraint.weight = Mathf.Lerp(1f, 0f, t / transitionTime);
            yield return null;
        }
        proceduralSlouchConstraint.weight = 0f;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    public override void OnExit()
    {
        StopAllCoroutines();
        
        if (headSnapConstraint != null) headSnapConstraint.weight = 0f;
        if (legIKConstraint != null) legIKConstraint.weight = 0f;
        if (proceduralLimpConstraint != null) proceduralLimpConstraint.weight = 0f;
        if (proceduralSlouchConstraint != null) proceduralSlouchConstraint.weight = 0f;
        
        if (legIKHint != null) legIKHint.localPosition = originalHintLocalPos;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        
        _isPerformingTell = false;
    }
}