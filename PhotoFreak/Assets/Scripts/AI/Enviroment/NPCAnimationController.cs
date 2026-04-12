using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private AIContext ctx;

    [Header("Locomotion Smoothing")]
    [SerializeField] private float smoothTime = 0.1f;
    private float velocityDamp;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int actionIDHash = Animator.StringToHash("ActionID");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        ctx = GetComponent<AIContext>();

        if (animator == null) Debug.LogWarning("No Animator found on children.");
    }

    void Update()
    {
        if (animator == null || ctx == null) return;

        UpdateLocomotion();
        UpdateActionState();
    }

    private void UpdateLocomotion()
    {
        if (ctx.currentActionState == NPCActionState.IDLE || ctx.currentActionState == NPCActionState.WALK)
        {
            bool isMoving = false;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                if ((agent.hasPath || agent.pathPending) && agent.remainingDistance > 0.1f) isMoving = true;
                else if (agent.velocity.sqrMagnitude > 0.05f) isMoving = true;
            }

            if (isMoving) ctx.currentActionState = NPCActionState.WALK;
            else ctx.currentActionState = NPCActionState.IDLE;
            
        }
    }

    private void UpdateActionState()
    {
        animator.SetInteger(actionIDHash, (int)ctx.currentActionState);
    }
}