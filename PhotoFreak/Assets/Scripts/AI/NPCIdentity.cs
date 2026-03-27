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

    void Awake()
    {
        ctx = GetComponent<AIContext>();
        ShowGuestModel(); 
    }

    // TODO: Setup an animation/particle system for model swapping to monster 
    public void Mutate(bool isSmartMonster)
    {
        ctx.isMonster = true;
        ctx.isOccupied = false; 

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
        if (guestModel is not null) guestModel.SetActive(false); 
        if (monsterModel is not null) monsterModel.SetActive(true);

        // TODO: Insert some particle or animation effect 
    }
}
