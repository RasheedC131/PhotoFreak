using UnityEngine;

public class ConsiderationWanderFree : Consideration
{
    [Header("Baseline Settings")]
    public float baseScore = 0.2f;

    protected override float EvaluateRawValue()
    {
        return baseScore;
    }
}