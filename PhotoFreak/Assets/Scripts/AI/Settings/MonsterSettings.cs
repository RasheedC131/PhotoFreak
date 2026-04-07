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

            return _instance;
        }
    }

    [Header("Base Settings")]
    [SerializeField] private float _walkSpeed = 3.5f; 

    [Header("Action Attack")]
    [SerializeField] private float _attackRange = 2.0f;

    [Header("Action Stalk")]
    [SerializeField] private float _stalkDistance = 4.0f; 
    [SerializeField] private float _stalkSpeed = 2.5f; 

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

    public float walkSpeed => _walkSpeed; 
    public float attackRange => _attackRange; 
    public float stalkDistance => _stalkDistance; 
    public float stalkSpeed => _stalkSpeed; 
    public float stalkSenseRadius => _stalkSenseRadius; 
    public float stalkIsolationCheckRadius => _stalkIsolationCheckRadius; 
    public float tellDuration => _tellDuration; 
    public float tellTimerMinTime => _tellTimerMinTime; 
    public float tellTimerMaxTime => _tellTimerMaxTime; 
    public float idleMaxFatigue => _idleMaxFatigue; 
}
