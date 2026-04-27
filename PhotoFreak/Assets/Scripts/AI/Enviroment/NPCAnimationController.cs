using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private AIContext ctx;

    [Header("Locomotion Settings")]
    [SerializeField] private float crossFadeDuration = 0.2f;
    [Tooltip("Minimum agent speed (m/s) that counts as walking.")]
    [SerializeField] private float moveSpeedThreshold = 0.15f;
    [Tooltip("Seconds the agent must stay below moveSpeedThreshold before the IDLE animation plays. Prevents idle flicker during brief path recalculations.")]
    [SerializeField] private float idleSettleDelay = 0.25f;

    [Header("Rotation Settings")]
    [Tooltip("How quickly the NPC body rotates to face its movement direction. Higher = snappier, lower = more gradual.")]
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Social Animation Settings")]
    [SerializeField] private string[] socialStateNames = new string[]
    {
        "AnnoyedHeadNod", "acknowledge", "RelievedSigh", "SarcasticHeadNod",
        "ThoughtfulHeadNod", "Dismisal", "ShakeHead", "HappyHand",
        "WeightShift", "YesHeadNod", "LookAway", "LengthyHeadNod", "HardHeadNod"
    };

    [SerializeField] private float minSocialAnimTime = 3f;
    [SerializeField] private float maxSocialAnimTime = 7f;

    private float socialAnimTimer = 0f;
    private float idleSettleTimer = 0f;

    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private int currentAnimHash;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        ctx = GetComponent<AIContext>();

        if (agent != null) agent.updateRotation = false;
    }

    void Update()
    {
        if (animator == null || ctx == null) return;

        UpdateLocomotionState();
        SmoothRotationToMovement();
        HandleAnimations();
    }

    private void UpdateLocomotionState()
    {
        if (ctx.currentActionState != NPCActionState.IDLE &&
            ctx.currentActionState != NPCActionState.WALK) return;

        bool isMoving = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            isMoving = agent.velocity.magnitude > moveSpeedThreshold;
        }

        if (isMoving)
        {
            idleSettleTimer = idleSettleDelay;
            ctx.currentActionState = NPCActionState.WALK;
        }
        else
        {

            idleSettleTimer -= Time.deltaTime;
            if (idleSettleTimer <= 0f)
                ctx.currentActionState = NPCActionState.IDLE;
        }
    }

    // rotate the NPC to face the next corner of its NavMesh path
    private void SmoothRotationToMovement()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (ctx.currentActionState != NPCActionState.WALK) return;
        if (agent.velocity.sqrMagnitude < moveSpeedThreshold * moveSpeedThreshold) return;

        Vector3 dir = agent.steeringTarget - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmoothSpeed);
    }

    private void HandleAnimations()
    {
        switch (ctx.currentActionState)
        {
            case NPCActionState.IDLE:
                PlayAnimation(idleHash);
                socialAnimTimer = 0f;
                break;

            case NPCActionState.WALK:
                PlayAnimation(walkHash);
                socialAnimTimer = 0f;
                break;

            case NPCActionState.SOCIALIZE:
                HandleSocialAnimations();
                break;
        }
    }

    private void HandleSocialAnimations()
    {
        socialAnimTimer -= Time.deltaTime;

        if (socialAnimTimer <= 0f)
        {
            if (socialStateNames.Length > 0)
            {
                int randomIndex = Random.Range(0, socialStateNames.Length);
                int targetHash = Animator.StringToHash(socialStateNames[randomIndex]);
                PlayAnimation(targetHash, true);
            }
            socialAnimTimer = Random.Range(minSocialAnimTime, maxSocialAnimTime);
        }
    }

    void OnEnable()
    {
        currentAnimHash = 0; 
    }

    private void PlayAnimation(int targetHash, bool forceTransition = false)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;

        if (forceTransition)
        {
            animator.CrossFade(targetHash, crossFadeDuration);
            currentAnimHash = targetHash;
            return;
        }

 
        if (currentAnimHash == targetHash)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != targetHash && !animator.IsInTransition(0))
            {
                animator.CrossFade(targetHash, crossFadeDuration);
            }
            return;
        }

        animator.CrossFade(targetHash, crossFadeDuration);
        currentAnimHash = targetHash;
    }
}