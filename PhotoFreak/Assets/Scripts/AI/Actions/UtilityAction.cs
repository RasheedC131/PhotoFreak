using UnityEngine;

public abstract class UtilityAction : MonoBehaviour
{
    private Consideration[] considerations;

    public abstract void ExecuteAction();

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