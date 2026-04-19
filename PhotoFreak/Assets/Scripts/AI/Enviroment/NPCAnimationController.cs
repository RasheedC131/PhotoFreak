using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private AIContext ctx;

    [Header("Locomotion Settings")]
    [SerializeField] private float crossFadeDuration = 0.2f;

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

    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private int currentAnimHash;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        ctx = GetComponent<AIContext>();
    }

    void Update()
    {
        if (animator == null || ctx == null) return;

        UpdateLocomotionState();
        HandleAnimations();
    }

    private void UpdateLocomotionState()
    {
        if (ctx.currentActionState == NPCActionState.IDLE || ctx.currentActionState == NPCActionState.WALK)
        {
            bool isMoving = false;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                // Trust the NavMesh path, ignore velocity to prevent jitter
                if (agent.pathPending) isMoving = true;
                else if (agent.hasPath && agent.remainingDistance > 0.1f) isMoving = true;
            }

            ctx.currentActionState = isMoving ? NPCActionState.WALK : NPCActionState.IDLE;
        }
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