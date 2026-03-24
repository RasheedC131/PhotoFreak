using UnityEngine;

// handles logic in-between guest to monster 
public class NPCIdentity : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject humanModel;
    [SerializeField] private GameObject defaultMonsterModelPrefab;
    [SerializeField] private GameObject killMonsterModelPrefab;

    [Header("Action References")]
    [SerializeField] private GameObject standardActionsFolder;
    [SerializeField] private GameObject monsterActionsFolder;

    private AIContext ctx; 

    void Awake()
    {
        ctx = GetComponent<AIContext>();
    }
    public void Mutate(bool isSmartMonster)
    {
        context.isMonster = true;
        context.isOccupied = false; 

        gameObject.tag = "Monster"; 
        PhotoTag tag = GetComponent<PhotoTag>();
        if (tag == null) tag = gameObject.AddComponent<PhotoTag>();
        tag.type = PhotoTag.SubjectType.Monster;
        tag.poseScore = 3;

        SwapModels();

        if (isSmartMonster)
        {
            if (standardActionsFolder != null) standardActionsFolder.SetActive(false);
            if (monsterActionsFolder != null) monsterActionsFolder.SetActive(true);
            Debug.Log($"{gameObject.name} mutated into a Smart Monster!");
        }
        else
        {
            if (standardActionsFolder != null) standardActionsFolder.SetActive(false);
            Debug.Log($"{gameObject.name} became a standard infected.");
        }
    }

    private void SwapModels()
    {
        if (humanModel != null) humanModel.SetActive(false);

        if (defaultMonsterModelPrefab != null)
        {
            GameObject newDefault = Instantiate(defaultMonsterModelPrefab, transform);
            newDefault.transform.localPosition = Vector3.zero;
            newDefault.SetActive(true);
        }

        if (killMonsterModelPrefab != null)
        {
            GameObject newKill = Instantiate(killMonsterModelPrefab, transform);
            newKill.transform.localPosition = Vector3.zero;
            newKill.SetActive(false);
        }
    }
}
