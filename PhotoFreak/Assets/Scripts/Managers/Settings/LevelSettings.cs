using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Managers/LevelSettings")]
public class LevelSettings : ScriptableObject
{
    private static LevelSettings _instance;
    public static LevelSettings Instance
    {
        get {
            if (_instance == null) _instance = Resources.Load<LevelSettings>("Managers/LevelSettings");
            return _instance;
        }
    }

    public enum InfectionMode { ONLY_STANDARD, ONLY_MONSTER, RANDOM } 

    [Header("Match Rules")]
    [SerializeField] private InfectionMode _levelInfectionMode = InfectionMode.ONLY_MONSTER;
    [Range(0, 100)] [SerializeField] private float _levelSmartAIChance = 50f;

    [Header("Tick Rates (Seconds)")]
    [SerializeField] private float _levelMonsterTickRate = 0.25f; 
    [SerializeField] private float _levelGuestTickRate = 0.5f; 

    public InfectionMode levelInfectionMode => _levelInfectionMode; 
    public float levelSmartAIChance => _levelSmartAIChance; 
    public float levelMonsterTickRate => _levelMonsterTickRate; 
    public float levelGuestTickRate => _levelGuestTickRate; 
}