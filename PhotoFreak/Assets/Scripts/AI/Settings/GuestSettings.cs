using UnityEngine;

[CreateAssetMenu(fileName = "GuestSettings", menuName = "AI/GuestSettings")]
public class GuestSettings : ScriptableObject
{
    private static GuestSettings _instance;
    public static GuestSettings Instance 
    {
        get 
        {
            if (_instance == null) _instance = Resources.Load<GuestSettings>("AI/GuestSettings");
            return _instance;
        }
    }

    [Header("Wander Nodes")]
    [SerializeField] private float _wanderNodeFleeDistance = 8f; 
    [SerializeField] private float _wanderMaxDistToDest = 1.5f; 
    [SerializeField] private float _wanderNodeSpreadRadius = 6.0f; 
    [SerializeField] private float _wanderMinWaitAtNode = 30.0f; 
    [SerializeField] private float _wanderMaxWaitAtNode = 60.0f; 
    [SerializeField] private float _wanderBaseSpeed = 2.5f; 
    [SerializeField] private float _wanderMinAcceleration = 5f; 
    [SerializeField] private float _wanderMaxAcceleration = 12f; 
    [SerializeField] private int _wanderNodeMaxCapacity = 5; 
    

    [Header("Action Socialize")]
    [SerializeField] private float _socialArrivalDistance = 2.0f;    
    [SerializeField] private float _socialTurnSpeed = 5f; 
    [SerializeField] private float _socialJoinHubRange = 15f; 
    [SerializeField] private float _socialConvRadius = 0.85f; 
    // considerations 
    [SerializeField] private float _socialMinTime = 20f;
    [SerializeField] private float _socialTimeDecay = 30f; 

    [Header("Action Isolate")]
    [SerializeField] private float _isolateTurnAngle = 30f;
    [SerializeField] private float _isolateKillNodeArrivalDist = 1.0f;
    [SerializeField] private float _playerKillNodeAvoidRadius = 8f;
    [SerializeField] private float _isolatePathfindingTimeout = 8f;

    [Header("Kill Room Wander Avoidance")]
    [SerializeField] private float _killRoomAvoidRadius = 8f;
    [Header("Kill Node Boredom")]

    [SerializeField] private float _killNodeBoredomTime = 20f;

    [Header("Action Idle")]
    [SerializeField] private float _idleMaxFatigue = 8.0f; 
    

    [Header("Fleeing and Threats")] [Tooltip("Used for both player fleeing and monster fleeing actions")]
    [SerializeField] private float _fleeDistance = 8f;
    [SerializeField] private float _fleePlayerSightRadius = 10f;
    [SerializeField] private float _fleePanicSpeed = 3.5f;

    [Header("Vigilance and Alert Scaling")]
    [SerializeField] private float _vigilanceRadiusMultiplier = 0.5f;
    [SerializeField] private float _alertRadiusMultiplier = 0.5f;
    [SerializeField] private float _panicFleeThreshold = 0.3f;

    [Header("Panic Contagion (Action_Flee)")]
    [SerializeField] private float _contagionRadius = 5f;
    [SerializeField] private float _contagionPanicAmount = 0.12f;
    [SerializeField] private float _contagionVigilanceGain = 0.04f;

    [Header("Panic Decay")]
    [SerializeField] private float _panicDecayRate = 0.3f;

    [Header("Alert Level — Social Suppression")]
    [SerializeField] private float _alertSuppressThreshold = 0.45f;
    [SerializeField] private float _groupAlertBoredomMultiplier = 2.5f;

    [Header("Time Anxiety — Social Suppression")]
    [SerializeField] private float _timeAnxietyStartRatio = 0.4f;
    [SerializeField] private float _timeAnxietyMinSocialScore = 0.5f;

    public float wanderNodeFleeDistance => _wanderNodeFleeDistance;
    public float wanderMaxDistToDest => _wanderMaxDistToDest; 
    public float wanderNodeSpreadRadius => _wanderNodeSpreadRadius; 
    public float wanderMinWaitAtNode => _wanderMinWaitAtNode; 
    public float wanderMaxWaitAtNode => _wanderMaxWaitAtNode; 
    public float wanderBaseSpeed => _wanderBaseSpeed; 
    public float wanderMinAcceleration => _wanderMinAcceleration; 
    public float wanderMaxAcceleration => _wanderMaxAcceleration; 
    public int wanderNodeMaxCapacity => _wanderNodeMaxCapacity; 
    public float socialArrivalDistance => _socialArrivalDistance; 
    public float socialTurnSpeed => _socialTurnSpeed; 
    public float socialJoinHubRange => _socialJoinHubRange; 
    public float socialConvRadius => _socialConvRadius; 
    public float socialTimeDecay => _socialTimeDecay; 
    public float socialMinTime => _socialMinTime;
    public float isolateTurnAngle => _isolateTurnAngle;
    public float isolateKillNodeArrivalDist => _isolateKillNodeArrivalDist;
    public float playerKillNodeAvoidRadius => _playerKillNodeAvoidRadius;
    public float isolatePathfindingTimeout => _isolatePathfindingTimeout;
    public float fleeDistance => _fleeDistance;
    public float fleePlayerSightRadius => _fleePlayerSightRadius;
    public float fleePanicSpeed => _fleePanicSpeed;
    public float vigilanceRadiusMultiplier => _vigilanceRadiusMultiplier;
    public float alertRadiusMultiplier => _alertRadiusMultiplier;
    public float panicFleeThreshold => _panicFleeThreshold;
    public float contagionRadius => _contagionRadius;
    public float contagionPanicAmount => _contagionPanicAmount;
    public float contagionVigilanceGain => _contagionVigilanceGain;
    public float panicDecayRate => _panicDecayRate;
    public float alertSuppressThreshold => _alertSuppressThreshold;
    public float groupAlertBoredomMultiplier => _groupAlertBoredomMultiplier;
    public float timeAnxietyStartRatio => _timeAnxietyStartRatio;
    public float timeAnxietyMinSocialScore => _timeAnxietyMinSocialScore;
    public float idleMaxFatigue => _idleMaxFatigue;
    public float killRoomAvoidRadius => _killRoomAvoidRadius;
    public float killNodeBoredomTime => _killNodeBoredomTime;
}
