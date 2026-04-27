using System.Collections;
using UnityEngine;

// handles logic in-between guest to monster 
public class NPCIdentity : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject guestModel;
    [SerializeField] private GameObject monsterModel;

    [Header("Action References")]
    [SerializeField] private GameObject standardActionsObj;
    [SerializeField] private GameObject monsterActionsObj;
    
    public bool isDisguised => guestModel != null && guestModel.activeSelf;

    private AIContext ctx;
    private MutationEffect mutationEffect;
    private AIBrain brain; // Cached reference

    void Awake()
    {
        ctx = GetComponent<AIContext>();
        mutationEffect = GetComponentInChildren<MutationEffect>();
        brain = GetComponent<AIBrain>(); 
        ShowGuestModel();
    }

    public void Mutate(bool isSmartMonster)
    {
        // change name of infected guest
        string prefix = isSmartMonster ? "[Monster] " : "[Infected] ";
        gameObject.name = prefix + gameObject.name;
        
        ctx.isMonster              = true;
        ctx.isOccupied             = false;
        ctx.currentVictim          = null;
        ctx.hasArrivedAtKillNode   = false;
        ctx.isAwareOfStalker       = false;

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled) agent.ResetPath();

        // --- THE BRAIN INTERRUPT ---
        // Force the AI Brain to immediately exit its current Guest action 
        if (brain != null && brain.currentAction != null)
        {
            brain.currentAction.OnExit();
            brain.currentAction = null;
        }

        // scoring logic
        gameObject.tag = "Monster";
        PhotoTag tag = GetComponent<PhotoTag>();
        if (tag is null) tag = gameObject.AddComponent<PhotoTag>();
        tag.type = PhotoTag.SubjectType.Monster;

        if (isSmartMonster)
        {
            // Toggle the action containers
            if (standardActionsObj != null) standardActionsObj.SetActive(false);
            if (monsterActionsObj != null) monsterActionsObj.SetActive(true);
            tag.poseScore = 3;
            Debug.Log($"{gameObject.name} mutated into a Smart Monster!");
        }
        else
        {
            if (standardActionsObj != null) standardActionsObj.SetActive(false);
            tag.poseScore = 1;
            Debug.Log($"{gameObject.name} became a standard infected.");
        }

        // Give the tell a fresh cooldown so the new monster doesn't immediately
        // perform a tell while still standing at the kill node.
        ConsiderationTellCooldown tellCooldown = GetComponentInChildren<ConsiderationTellCooldown>(true);
        if (tellCooldown != null) tellCooldown.ResetTimer();

        // Re-cache the brain's array. Because we don't pass 'true' into this method,
        // it automatically ignores the disabled Guest actions!
        if (brain != null)
        {
            brain.availableActions = GetComponentsInChildren<UtilityAction>();
        }
    }

    public void ShowGuestModel()
    {
        if (guestModel is not null) guestModel.SetActive(true); 
        if (monsterModel is not null) monsterModel.SetActive(false); 
    }

    public void ShowMonsterModel()
    {
        StartCoroutine(MutationSequence());
    }

    private IEnumerator MutationSequence()
    {
        // Play the circles first so they're visible around the guest model.
        if (mutationEffect != null) mutationEffect.Play();

        // Wait until the effect is roughly halfway done, then swap the model
        // so the reveal happens while the circles are still clearly visible.
        float switchDelay = mutationEffect != null ? mutationEffect.Duration * 0.4f : 0f;
        yield return new WaitForSeconds(switchDelay);

        if (guestModel is not null) guestModel.SetActive(false);
        if (monsterModel is not null) monsterModel.SetActive(true);
        // Circles continue and fade out naturally over the remaining duration.
    }
}