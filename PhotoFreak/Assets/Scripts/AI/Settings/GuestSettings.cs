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

    [Header("Action Isolate")]
    [SerializeField] private float _isolateTurnAngle = 30f;
    [SerializeField] private float _isolateKillNodeArrivalDist = 1.0f; 

    [Header("Fleeing and Threats")] [Tooltip("Used for both player fleeing and monster fleeing actions")]
    [SerializeField] private float _fleeDistance = 8f; 
    [SerializeField] private float _fleePlayerSightRadius = 10f; 
    [SerializeField] private float _fleePanicSpeed = 3.5f; 

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
    public float isolateTurnAngle => _isolateTurnAngle; 
    public float isolateKillNodeArrivalDist => _isolateKillNodeArrivalDist; 
    public float fleeDistance => _fleeDistance; 
    public float fleePlayerSightRadius => _fleePlayerSightRadius; 
    public float fleePanicSpeed => _fleePanicSpeed; 

}
