using UnityEngine;

[CreateAssetMenu(fileName = "MonsterWeights", menuName = "AI/MonsterWeights")]
public class MonsterWeights : ScriptableObject
{
    private static MonsterWeights _instance;
    public static MonsterWeights Instance {
        get {
            if (_instance == null) _instance = Resources.Load<MonsterWeights>("MonsterWeights");
            return _instance;
        }
    }
    
    [Header("Consideration Stalking")]
    [SerializeField] private float _stalkMinWeight = 0.4f;


    [Header("Consideration Prey Nearby")]
    [SerializeField] private float _preySpottedWeight = 0.8f;

    [Header("Consideration WanderFree")]
    [SerializeField] private float _wanderFreeWeight = 0.2f; 

    public float stalkMinWeight => _stalkMinWeight; 
    public float preySpottedWeight => _preySpottedWeight; 
    public float wanderFreeWeight => _wanderFreeWeight; 
}
