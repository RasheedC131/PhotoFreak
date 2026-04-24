using UnityEngine;

// think about whether monster should attack the player 
public class ConsiderationHuntPlayer : Consideration
{
    private AIContext    ctx;
    private MonsterSettings ms;
    private Transform   playerTransform;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms  = MonsterSettings.Instance;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected override float EvaluateRawValue()
    {
        if (ctx == null || !ctx.isMonster)  return 0f;
        if (CrowdStateManager.Instance == null || !CrowdStateManager.Instance.IsFinalPanic) return 0f;
        if (playerTransform == null) return 0f;

        float dist = Vector3.Distance(ctx.transform.position, playerTransform.position);
        if (dist > ms.stalkSenseRadius) return 0f;

  
        float proximityBonus = 1f - Mathf.Clamp01(dist / ms.stalkSenseRadius);
        return 0.88f + proximityBonus * 0.08f; // range: 0.88 – 0.96
    }
}
