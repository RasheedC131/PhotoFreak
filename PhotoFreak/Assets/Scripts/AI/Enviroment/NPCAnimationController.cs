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

        // Disable the agent's built-in rotation so we can smooth it ourselves.
        // Without this, avoidance steering snaps the body direction instantly,
        // which breaks the walk cycle when an NPC steers around another.
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
        // Only manage IDLE / WALK — leave SOCIALIZE and any other states untouched.
        if (ctx.currentActionState != NPCActionState.IDLE &&
            ctx.currentActionState != NPCActionState.WALK) return;

        bool isMoving = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // Velocity is the ground truth: it reflects what the character is
            // physically doing right now, including avoidance steering.  The old
            // path-pending / remainingDistance approach had a 1–2 frame gap every
            // time SetDestination was called, causing the idle animation to flash.
            isMoving = agent.velocity.magnitude > moveSpeedThreshold;
        }

        if (isMoving)
        {
            idleSettleTimer = idleSettleDelay;
            ctx.currentActionState = NPCActionState.WALK;
        }
        else
        {
            // Don't flip to IDLE immediately — wait for the settle timer so that
            // brief stops during path recalculation don't show a frame of idle.
            idleSettleTimer -= Time.deltaTime;
            if (idleSettleTimer <= 0f)
                ctx.currentActionState = NPCActionState.IDLE;
        }
    }

    // Smoothly rotate the NPC to face the next corner of its NavMesh path.
    // Using steeringTarget (the next waypoint) rather than raw velocity prevents
    // jitter from the avoidance system causing sudden body direction snaps.
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

    private void PlayAnimation(int targetHash, bool forceTransition = false)
    {
        if (currentAnimHash == targetHash && !forceTransition) return;
        animator.CrossFade(targetHash, crossFadeDuration);
        currentAnimHash = targetHash;
    }
}