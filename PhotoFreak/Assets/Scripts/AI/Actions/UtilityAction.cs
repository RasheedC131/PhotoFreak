using UnityEngine;

// base class that serves as actions that the npcs can take 
public abstract class UtilityAction : MonoBehaviour
{
    [Header("Action Details")]
    public string actionName;      
    
    // Consideration scripts (e.g., timer, dist. from target, etc.) 
    public Consideration[] considerations; 

    // calculate the score for the actions 
    public float CalculateUtilityScore()
    {
        if (considerations == null || considerations.Length == 0) return 0f; 
        
        float finalScore = 1f; 
        
        foreach (Consideration cons in considerations)
        {
            float currScore = cons.GetScore(); 
            finalScore *= currScore; 

            if (finalScore == 0) break; 
        }

        return finalScore; 
    }

    // to be implemented in derived class 
    public abstract void ExecuteAction(); 
}
