using UnityEngine;

[CreateAssetMenu(fileName = "MonsterWeights", menuName = "AI/MonsterWeights")]
public class MonsterWeights : ScriptableObject
{
    private static MonsterWeights _instance;
    public static MonsterWeights Instance {
        get {
            if (_instance == null) _instance = Resources.Load<MonsterWeights>("AI/MonsterWeights");
            return _instance;
        }
    }
    
    [Header("Consideration Stalking")]
    [SerializeField] [Range(0, 1)] private float _stalkMinWeight = 0.4f;

    [Header("Consideration Prey Nearby")]
    [SerializeField] [Range(0, 1)] private float _preySpottedWeight = 0.8f;

    [Header("Consideration WanderFree")]
    [SerializeField] [Range(0, 1)] private float _wanderFreeWeight = 0.2f; 

    [Header("Consideration Idle")]
    [SerializeField] [Range(0, 1)] private float _idleWeight = 0.9f; 

    public float stalkMinWeight => _stalkMinWeight; 
    public float preySpottedWeight => _preySpottedWeight; 
    public float wanderFreeWeight => _wanderFreeWeight; 
    public float idleWeight => _idleWeight; 
}
