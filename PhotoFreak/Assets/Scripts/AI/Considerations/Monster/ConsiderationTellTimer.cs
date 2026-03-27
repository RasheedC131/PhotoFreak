using UnityEngine;

public class Consideration_TellCooldown : Consideration
{
    [Header("Settings")]
    public float minTimeBetweenTells = 15f; 
    public float maxTimeBetweenTells = 30f; 
    private float timer = 0f; 

    protected override float EvaluateRawValue()
    {
        timer += Time.deltaTime; 
        float tellTime = Random.Range(minTimeBetweenTells, maxTimeBetweenTells); 
        float score = Mathf.Clamp01(timer / tellTime);

        return score; 
    }

    public void ResetTimer()
    {
        timer = 0f; 
    }
}