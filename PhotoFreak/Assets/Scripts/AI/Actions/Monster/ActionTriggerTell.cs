using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations.Rigging; 


// TODO: maybe add more tells when standing still or moving to make it less obvious 
public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private NavMeshAgent agent; 
    private MonsterSettings ms; 
    private ConsiderationTellCooldown tellTimer; 
    private Animator animator;

    private bool _isPerformingTell = false; 
    public bool isPerformingTell => _isPerformingTell; 

    [Header("IK Rigging")]
    public MultiAimConstraint headSnapConstraint; 
    public Transform headSnapTarget; 
    
    public TwoBoneIKConstraint legIKConstraint;
    public Transform legIKHint;
    private Vector3 originalHintLocalPos;
    public OverrideTransform proceduralLimpConstraint; 
    public OverrideTransform proceduralSlouchConstraint;


 
    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        agent = GetComponentInParent<NavMeshAgent>();
        animator = GetComponentInParent<Animator>(); 
        ms = MonsterSettings.Instance; 
        tellTimer = GetComponent<ConsiderationTellCooldown>(); 

        if (legIKHint != null) originalHintLocalPos = legIKHint.localPosition;
    }

    public override void ExecuteAction()
    {
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

        int tellChoice = Random.Range(0, 4);
        switch (tellChoice)
        {
            case 0: yield return StartCoroutine(HeadSnapRoutine()); break;
            case 1: yield return StartCoroutine(HyperextendJointRoutine()); break;
            case 2: yield return StartCoroutine(HeavyLimpRoutine()); break;
            case 3: yield return StartCoroutine(SlouchRoutine()); break;
        }
        
        _isPerformingTell = false; 
    }

    private IEnumerator HeadSnapRoutine()
    {
        Debug.Log("Room Scan Routine Triggered!");
        if (headSnapConstraint == null || headSnapTarget == null) 
        {
            Debug.LogWarning("Missing Head Snap IK references.");
            yield break;
        }

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
        float scanSpeed = 5f; 
        float scanWidth = 4f; 

        Vector3 scanRightVector = Vector3.Cross(Vector3.up, baseForward).normalized;

        while (scanTimer < ms.tellDuration)
        {
            scanTimer += Time.deltaTime;
            Vector3 sweepOffset = scanRightVector * (Mathf.Sin(scanTimer * scanSpeed) * scanWidth);
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
        Debug.Log("Room Scan Routine Completed.");
    }

    private IEnumerator HyperextendJointRoutine()
    {
        Debug.Log("Tell: Hyperextend Joint Routine Triggered");
        if (legIKConstraint == null || legIKHint == null) 
        {
            Debug.LogWarning("Missing Leg IK references.");
            yield break;
        }

        legIKHint.localPosition = new Vector3(originalHintLocalPos.x, originalHintLocalPos.y, -originalHintLocalPos.z);
        legIKConstraint.weight = 1f;
        yield return new WaitForSeconds(ms.tellDuration);
        legIKConstraint.weight = 0f;
        legIKHint.localPosition = originalHintLocalPos;
        
    }

    private IEnumerator HeavyLimpRoutine()
    {
        Debug.Log("Tell: Limp Routine Triggered!");

        if (proceduralLimpConstraint == null) 
        {
            Debug.LogError("ERROR: Procedural Limp Constraint missing in the Inspector");
            yield break; 
        }

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

    private IEnumerator SlouchRoutine()
    {
        Debug.Log("Slouch Routine Triggered!");

        if (proceduralSlouchConstraint == null) 
        {
            Debug.LogError("ERROR: Procedural Slouch Constraint missing in the Inspector");
            yield break; 
        }

      
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
        Debug.Log("Slouch Routine Completed.");
    }

    public override void OnExit()
    {
        if (_isPerformingTell)
        {
            Debug.LogWarning($"[{ctx.gameObject.name}] WARNING: AI Brain interrupted the Tell before it could finish! Resetting IK to 0.");
        }

        StopAllCoroutines();
        
        if (headSnapConstraint != null) headSnapConstraint.weight = 0f;
        
        if (legIKConstraint != null) 
        {
            legIKConstraint.weight = 0f;
            if (legIKHint != null) legIKHint.localPosition = originalHintLocalPos;
        }

        if (proceduralLimpConstraint != null) proceduralLimpConstraint.weight = 0f;
        
        if (proceduralSlouchConstraint != null) proceduralSlouchConstraint.weight = 0f;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        
        _isPerformingTell = false;
    }
}