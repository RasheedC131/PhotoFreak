using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSettings", menuName = "AI/Settings/MonsterSettings")]
public class MonsterSettings : ScriptableObject
{
    private static MonsterSettings _instance;
    public static MonsterSettings Instance
    {
        get {
            if (_instance == null) _instance = Resources.Load<MonsterSettings>("MonsterSettings");
            return _instance;
        }
    }

    [Header("Physical Attributes")]
    public float sightRadius = 15f;
    public float attackRange = 2.5f;
    public float stalkDurationLimit = 10f;
    public float walkSpeed = 1.5f;
    public float huntSpeed = 2.5f;
}