using UnityEngine; 

[CreateAssetMenu(fileName = "GuestWeights", menuName = "AI/GuestWeights")]
public class GuestWeights : ScriptableObject 
{
    private static GuestWeights _instance;
    public static GuestWeights Instance 
    {
        get
        {
            if (_instance == null) _instance = Resources.Load<GuestWeights>("AI/GuestWeights");
            return _instance;
        }
    }

    [Header("AI Personality")][Tooltip("Used for adding offset to certain weights to break ties")]
    [SerializeField][Range(0, 0.5f)] private float _maxPersonalityOffset = 0.1f; 

    [Header("Consideration Group Available")]
    [SerializeField] [Range(0, 1)] private float _groupAvailWeight = 0.7f;
    [SerializeField] private float _groupSocialSightDist = 20.0f; 

    [Header("Consideration Wander Nodes")]
    [SerializeField] [Range(0, 1)] private float _wanderNodesWeight = 0.75f; 
    [SerializeField] [Range(0, 1)] private float _wanderNodesCeilingWeight = 0.8f; 
    [SerializeField] [Range(0, 1)] private float _wanderNodesCommittedWeight = 0.85f; 

    [Header("Consideration Social Weight")]
    [SerializeField] [Range(0, 1)] private float _socialWeight = 0.88f;
  
    [Header("Consideration Is Solo")]
    [SerializeField] [Range(0, 1)] private float _soloWeight = 0.5f;
    [SerializeField] [Range(0, 1)] private float _soloCommittedActionWeight = 0.65f; 
    [SerializeField] [Range(0, 1)] private float _soloCommittedWeight = 0.7f; 

    [Header("Consideration Is Stalked")] 
    [SerializeField] [Range(0, 1)] private float _isStalkedWeight = 0.9f; 

    [Header("Consideration Threats")][Tooltip("Used for fleeing from monster")]
    [SerializeField] [Range(0, 1)] private float _monsterSpottedWeight = 1.0f; 
    [SerializeField] private float _guestPanicDistance = 30f; 
    [SerializeField] [Range(0, 1)] private float _playerSpottedWeight = 0.95f;  

    [Header("Consideration Idle")]
    [SerializeField] [Range(0, 1)] private float _idleWeight = 0.4f; 

    public float maxPersonalityOffset => _maxPersonalityOffset; 
    public float groupAvailWeight => _groupAvailWeight; 
    public float groupSocialSightDist => _groupSocialSightDist; 
    public float wanderNodesWeight => _wanderNodesWeight; 
    public float wanderNodesCeilingWeight => _wanderNodesCeilingWeight; 
    public float wanderNodesCommittedWeight => _wanderNodesCommittedWeight; 
    public float socialWeight => _socialWeight; 
    public float soloWeight => _soloWeight; 
    public float soloCommittedActionWeight => _soloCommittedActionWeight; 
    public float soloComittedWeight => _soloCommittedWeight; 
    public float isStalkedWeight => _isStalkedWeight; 
    public float monsterSpottedWeight => _monsterSpottedWeight; 
    public float guestPanicDistance => guestPanicDistance; 
    public float playerSpottedWeight => _playerSpottedWeight; 
    public float idleWeight => _idleWeight; 
}