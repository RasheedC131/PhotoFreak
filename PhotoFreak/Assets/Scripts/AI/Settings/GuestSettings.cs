using UnityEngine;

[CreateAssetMenu(fileName = "GuestSettings", menuName = "AI/Settings/GuestSettings")]
public class GuestSettings : ScriptableObject
{
    private static GuestWeights _instance;
    public static GuestWeights Instance {
        get {
            if (_instance == null) _instance = Resources.Load<GuestSettings>("GuestSettings");
            return _instance;
        }
    }

    public float fleeDistance = 8f; 

}
