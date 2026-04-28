using UnityEngine;

public abstract class Consideration : MonoBehaviour
{
    public AnimationCurve responseCurve = AnimationCurve.Linear(0, 0, 1, 1); 

    // calculates a score to decide whether or not to take the action 
    public float GetScore()
    {
        float rawValue = EvaluateRawValue(); 
        float normalizedValue = Mathf.Clamp01(rawValue);
        return responseCurve.Evaluate(normalizedValue); 
    }

    // gets implemented in derived class 
    protected abstract float EvaluateRawValue(); 
}
