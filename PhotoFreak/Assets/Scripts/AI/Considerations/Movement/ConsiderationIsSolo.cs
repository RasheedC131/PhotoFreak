using UnityEngine;

public class ConsiderationIsSolo : Consideration
{
    private AIContext ctx; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>(); 
    }

    protected override float EvaluateRawValue()
    {
        // return a full or empty score depending on group size 
        return ctx.groupTotalSize <= 1? 1f: 0f; 
    }
}
