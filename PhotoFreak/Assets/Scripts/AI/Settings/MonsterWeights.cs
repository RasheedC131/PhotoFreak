using UnityEngine;

[CreateAssetMenu(fileName = "MonsterWeight", menuName = "AI/Settings/MonsterWeight")]
public class MonsterWeights : ScriptableObject
{
    private static MonsterWeights _instance;
    public static MonsterWeights Instance {
        get {
            if (_instance == null) _instance = Resources.Load<MonsterWeights>("MonsterWeights");
            return _instance;
        }
    }
    public float stalkMax = 1.0f;
    public float stalkMin = 0.4f;
    public float preySpotted = 0.8f;
}
