using UnityEngine; 

[CreateAssetMenu(fileName = "MonsterSettings", menuName = "AI/MonsterSettings")]
public class MonsterSettings : ScriptableObject
{
    private static MonsterSettings _instance;
    public static MonsterSettings Instance
    {
        get 
        {
            if (_instance == null) _instance = Resources.Load<MonsterSettings>("AI/MonsterSettings");

            if (_instance == null)
                {
                    Debug.LogError("CRITICAL: Could not find the MonsterSettings asset in 'Resources/AI/'");
                    
                }

            return _instance;
        }
    }

    [Header("Base Settings")]
    [SerializeField] private float _walkSpeed = 3.5f; 

    [Header("Action Attack")]
    [SerializeField] private float _attackRange = 2.0f;
    [SerializeField] private float _witnessRadius = 20f;
    [SerializeField] private float _killRoomWitnessRadius = 5f;

    [Header("Action Stalk")]
    [SerializeField] private float _stalkDistance = 10.0f; 
    [SerializeField] private float _stalkSpeed = 2.5f; 
    [SerializeField] private float _stalkDuration = 60f; 
    
    [Header("Action Idle")]
    [SerializeField] private float _idleMaxFatigue = 8.0f; 

    [Header("Consideration Stalk")]
    [SerializeField] private float _stalkSenseRadius = 100f; 
    [SerializeField] private float _stalkIsolationCheckRadius = 5f;

    [Header("Action Trigger Tell")] 
    [SerializeField] private float _tellDuration = 2.0f;  

    [Header("Consideration Tell Timer")]
    [SerializeField] private float _tellTimerMinTime = 15f;
    [SerializeField] private float _tellTimerMaxTime = 30f;

    [Header("Revealed Chase")]
    [SerializeField] private float _revealedSpeed = 5.5f;

    [Header("Action Hunt Player (Final Panic)")]
    [SerializeField] private float _huntPlayerSpeed = 6.0f;

    public float walkSpeed => _walkSpeed; 
    public float attackRange => _attackRange;
    public float witnessRadius => _witnessRadius;
    public float killRoomWitnessRadius => _killRoomWitnessRadius;
    public float stalkDistance => _stalkDistance; 
    public float stalkSpeed => _stalkSpeed; 
    public float stalkDuration => _stalkDuration; 
    public float stalkSenseRadius => _stalkSenseRadius; 
    public float stalkIsolationCheckRadius => _stalkIsolationCheckRadius; 
    public float tellDuration => _tellDuration;
    public float tellTimerMinTime => _tellTimerMinTime;
    public float tellTimerMaxTime => _tellTimerMaxTime;
    public float revealedSpeed => _revealedSpeed;
    public float huntPlayerSpeed => _huntPlayerSpeed;
    public float idleMaxFatigue => _idleMaxFatigue;
}
