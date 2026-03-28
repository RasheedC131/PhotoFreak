using UnityEngine; 

[CreateAssetMenu(fileName = "GuestWeights", menuName = "AI/GuestWeights")]
public class GuestWeights : ScriptableObject 
{
    private static GuestWeights _instance;
    public static GuestWeights Instance 
    {
        get
        {
            if (_instance == null) _instance = Resources.Load<GuestWeights>("GuestWeights");
            return _instance;
        }
    }

    [Header("Consideration Group Available")]
    [SerializeField] [Range(0, 1)] private float _groupAvailWeight = 0.7f;

    [Header("Consideration Wander Nodes")]
    [SerializeField] [Range(0, 1)] private float _wanderNodesWeight = 0.4f; 
  
    [Header("Consideration Is Solo")]
    [SerializeField] [Range(0, 1)] private float _soloWeight = 0.5f;

    [Header("Consideration Is Stalked")] 
    [SerializeField] [Range(0, 1)] private float _isStalkedWeight = 0.8f; 

    [Header("Consideration Threats")][Tooltip("Used for fleeing from monster")]
    [SerializeField] [Range(0, 1)] private float _monsterSpottedWeight = 1.0f; 
    [SerializeField] [Range(0, 1)] private float _playerSpottedWeight = 0.9f;  


    public float groupAvailWeight => _groupAvailWeight; 
    public float wanderNodesWeight => _wanderNodesWeight; 
    public float soloWeight => _soloWeight; 
    public float isStalkedWeight => _isStalkedWeight; 
    public float monsterSpottedWeight => _monsterSpottedWeight; 
    public float playerSpottedWeight => _playerSpottedWeight; 

}