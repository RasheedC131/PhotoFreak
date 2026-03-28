using UnityEngine; 

[CreateAssetMenu(fileName = "GuestWeights", menuName = "AI/Settings/GuestWeights")]
public class GuestWeights : ScriptableObject {
    private static GuestWeights _instance;
    public static GuestWeights Instance {
        get {
            if (_instance == null) _instance = Resources.Load<GuestWeights>("GuestWeights");
            return _instance;
        }
    }
    public float socialWeight = 0.5f;
    public float isolateWeight = 0.9f;
}