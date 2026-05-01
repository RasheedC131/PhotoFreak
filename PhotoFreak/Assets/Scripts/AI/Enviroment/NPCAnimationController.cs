using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private NPCIdentity identity;

    [Header("Locomotion Settings")]
    [SerializeField] private float crossFadeDuration = 0.2f;
    [Tooltip("Minimum agent speed (m/s) that counts as walking.")]
    [SerializeField] private float moveSpeedThreshold = 0.15f;
    [Tooltip("Seconds the agent must stay below moveSpeedThreshold before the IDLE animation plays. Prevents idle flicker during brief path recalculations.")]
    [SerializeField] private float idleSettleDelay = 0.25f;

    [Header("Rotation Settings")]
    [Tooltip("How quickly the NPC body rotates to face its movement direction. Higher = snappier, lower = more gradual.")]
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Guest Animation Settings")]
    [Tooltip("Leave empty to auto-discover the Animator on the guest model child.")]
    [SerializeField] private Animator guestAnimator;
    [SerializeField] private string[] socialStateNames = new string[]
    {
        "AnnoyedHeadNod", "acknowledge", "RelievedSigh", "SarcasticHeadNod",
        "ThoughtfulHeadNod", "Dismisal", "ShakeHead", "HappyHand",
        "WeightShift", "YesHeadNod", "LookAway", "LengthyHeadNod", "HardHeadNod"
    };
    [SerializeField] private float minSocialAnimTime = 3f;
    [SerializeField] private float maxSocialAnimTime = 7f;

    [Header("Monster Animation Settings")]
    [Tooltip("Animator on the monster model child — drag it in here.")]
    [SerializeField] private Animator monsterAnimator;
    [Tooltip("State name in the monster Animator Controller for walking.")]
    [SerializeField] private string monsterWalkStateName = "Walk";
    [Tooltip("State name in the monster Animator Controller for idle. Leave empty to simply freeze the animator when the monster stops moving.")]
    [SerializeField] private string monsterIdleStateName = "";

    // Guest hashes
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private int currentGuestAnimHash;

    // Monster hashes — built from the inspector strings in Start()
    private int monsterWalkHash;
    private int monsterIdleHash;
    private int currentMonsterAnimHash;

    private float socialAnimTimer = 0f;
    private float idleSettleTimer = 0f;

    // Returns whichever animator belongs to the currently visible model.
    private Animator ActiveAnimator => (identity != null && !identity.isDisguised && monsterAnimator != null)
        ? monsterAnimator
        : guestAnimator;

    // Ref to the hash tracker for the active animator.
    private int CurrentAnimHash
    {
        get => (identity != null && !identity.isDisguised && monsterAnimator != null)
            ? currentMonsterAnimHash : currentGuestAnimHash;
        set
        {
            if (identity != null && !identity.isDisguised && monsterAnimator != null)
                currentMonsterAnimHash = value;
            else
                currentGuestAnimHash = value;
        }
    }

    void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        ctx      = GetComponent<AIContext>();
        identity = GetComponent<NPCIdentity>();

        // Auto-discover the guest animator if not assigned in the inspector.
        if (guestAnimator == null)
            guestAnimator = GetComponentInChildren<Animator>();

        monsterWalkHash = Animator.StringToHash(monsterWalkStateName);
        monsterIdleHash = string.IsNullOrEmpty(monsterIdleStateName) ? 0 : Animator.StringToHash(monsterIdleStateName);

        if (agent != null) agent.updateRotation = false;
    }

    void Update()
    {
        if (ctx == null) return;

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
        bool isMonster = identity != null && !identity.isDisguised && monsterAnimator != null;

        switch (ctx.currentActionState)
        {
            case NPCActionState.IDLE:
                if (isMonster)
                    SetMonsterPaused(true);   // freeze on last frame — no idle anim needed
                else
                    PlayAnimation(idleHash);
                socialAnimTimer = 0f;
                break;

            case NPCActionState.WALK:
                if (isMonster)
                {
                    SetMonsterPaused(false);  // resume before playing walk
                    PlayAnimation(monsterWalkHash);
                }
                else
                    PlayAnimation(walkHash);
                socialAnimTimer = 0f;
                break;

            case NPCActionState.SOCIALIZE:
                // Monsters don't socialise — freeze in place.
                if (isMonster)
                    SetMonsterPaused(true);
                else
                    HandleSocialAnimations();
                break;
        }
    }

    // Pausing via speed=0 freezes the animator on its current frame without
    // disabling the component, so blend trees and transitions still resolve correctly
    // the moment the monster starts moving again.
    private void SetMonsterPaused(bool paused)
    {
        if (monsterAnimator != null)
            monsterAnimator.speed = paused ? 0f : 1f;
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
        currentGuestAnimHash   = 0;
        currentMonsterAnimHash = 0;
    }

    private void PlayAnimation(int targetHash, bool forceTransition = false)
    {
        Animator anim = ActiveAnimator;
        if (anim == null || !anim.isActiveAndEnabled || anim.runtimeAnimatorController == null) return;

        if (forceTransition)
        {
            anim.CrossFade(targetHash, crossFadeDuration);
            CurrentAnimHash = targetHash;
            return;
        }

        if (CurrentAnimHash == targetHash)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != targetHash && !anim.IsInTransition(0))
            {
                anim.CrossFade(targetHash, crossFadeDuration);
            }
            return;
        }

        anim.CrossFade(targetHash, crossFadeDuration);
        CurrentAnimHash = targetHash;
    }
}