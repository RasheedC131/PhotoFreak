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

    private AIContext ctx;
    private MutationEffect mutationEffect;

    void Awake()
    {
        ctx = GetComponent<AIContext>();
        mutationEffect = GetComponentInChildren<MutationEffect>();
        ShowGuestModel();
    }

    public void Mutate(bool isSmartMonster)
    {
        // change name of infected guest 
        string prefix = isSmartMonster ? "[Monster] " : "[Infected] ";
        gameObject.name = prefix + gameObject.name;
        
        ctx.isMonster = true;
        ctx.isOccupied = false;
        ctx.currentVictim = null;

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled) agent.ResetPath();

        // scoring logic
        gameObject.tag = "Monster";
        PhotoTag tag = GetComponent<PhotoTag>();
        if (tag is null) tag = gameObject.AddComponent<PhotoTag>();
        tag.type = PhotoTag.SubjectType.Monster;

        if (isSmartMonster)
        {
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

        GetComponent<AIBrain>().availableActions = GetComponentsInChildren<UtilityAction>();
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
