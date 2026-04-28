using UnityEngine;

public abstract class UtilityAction : MonoBehaviour
{
    [Tooltip("Tick this on every action that belongs to the monster behaviour set " +
             "(ActionStalk, ActionAttack, ActionHuntPlayer, etc.). " +
             "Leave unticked for guest actions (ActionWanderNodes, ActionSocialize, ActionIsolate, etc.). " +
             "AIBrain uses this flag to only score actions that match the NPC's current role.")]
    public bool isMonsterAction = false;

    private Consideration[] considerations;

    public abstract void ExecuteAction();
    public virtual void OnEnter() {} 
    public virtual void OnExit() {} 

    public float CalculateUtilityScore()
    {
        if (considerations == null)
        {
            considerations = GetComponents<Consideration>();
        }

        if (considerations.Length == 0) return 0f;

        float finalScore = 1.0f;

        foreach (Consideration consideration in considerations)
        {
            if (consideration == null) continue; 

            try 
            {
                float score = consideration.GetScore(); 
                
                if (score == 0) return 0f; 
                finalScore *= score;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AI ERROR] The consideration '{consideration.GetType().Name}' on the object '{gameObject.name}' crashed. Error: {e.Message}");
                return 0f; 
            }
        }

        return finalScore;
    }
}