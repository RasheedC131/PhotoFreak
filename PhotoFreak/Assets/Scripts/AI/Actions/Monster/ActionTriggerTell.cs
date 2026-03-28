using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private MonsterSettings ms; 
    private NPCIdentity identity; 

    private bool isPreformingTell = false;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        identity = GetComponentInParent<NPCIdentity>();
    }

    public override void ExecuteAction()
    {
        if (!isPreformingTell)
        {
            StartCoroutine(PreformTellRoutine()); 
        }
    }

    // TODO: Implement actual tells 
    private IEnumerator PreformTellRoutine()
    {
        isPreformingTell = true; 
        ctx.agent.isStopped = true; 

        Debug.Log($"{ctx.gameObject.name} is performing a monster tell");

        yield return new WaitForSeconds(ms.tellDuration); 
        
        ctx.currentStalkTimer = 0f; 
        ctx.agent.isStopped = false; 
        isPreformingTell = false; 
    }
}