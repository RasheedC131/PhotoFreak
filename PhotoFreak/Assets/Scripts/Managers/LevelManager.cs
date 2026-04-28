using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    public enum InfectionMode { ONLY_STANDARD, ONLY_MONSTER, RANDOM } 

    [Header("Infection Settings")]
    public InfectionMode infectionMode = InfectionMode.ONLY_MONSTER;
    public float smartAIChance = 50f;       

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void HandleInfection(AIContext victim, AIContext attacker)
    {
        if (victim.isMonster) return; 
        bool attackerIsSmart = attacker != null && attacker.isMonster;

        NPCIdentity victimIdentity = victim.GetComponent<NPCIdentity>();
        if (victimIdentity == null)
        {
            Debug.LogError("Victim is missing their NPCIdentity script!");
            return;
        }

        if (!attackerIsSmart)
        {
            victimIdentity.Mutate(false); // Mutate into standard
            return; 
        }

        bool makeSmart = false; 
        switch (infectionMode)
        {
            case InfectionMode.ONLY_STANDARD:   makeSmart = false;                                  break; 
            case InfectionMode.ONLY_MONSTER:    makeSmart = true;                                   break; 
            case InfectionMode.RANDOM:          makeSmart = Random.Range(0f, 100f) < smartAIChance; break; 
        }

        // Issue the final command
        victimIdentity.Mutate(makeSmart); 
    }
}