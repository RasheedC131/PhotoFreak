using UnityEngine;

public class ConsiderationWanderFree : Consideration
{
    MonsterWeights ms; 

    void Awake()
    {
        ms = MonsterWeights.Instance; 
    }

    protected override float EvaluateRawValue()
    {
        return ms.wanderFreeWeight;
    }
}