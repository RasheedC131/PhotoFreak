using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private MonsterSettings ms; 
    private NPCIdentity identity; 

    private bool _isPerformingTell = false; 
    private ConsiderationTellCooldown tellTimer; 
    public bool isPerformingTell => _isPerformingTell; 

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
        ms = MonsterSettings.Instance; 
        identity = GetComponentInParent<NPCIdentity>();
        tellTimer = GetComponent<ConsiderationTellCooldown>(); 
        if (tellTimer == null) Debug.LogError("ConsiderationTellCooldown not found on " + gameObject.name);

    }

    public override void ExecuteAction()
    {
        if (!isPerformingTell)
        {
            StartCoroutine(PreformTellRoutine()); 
        }
    }

    // TODO: Implement actual tells 
    private IEnumerator PreformTellRoutine()
    {
        _isPerformingTell = true; 
        ctx.agent.isStopped = true; 
        tellTimer.ResetTimer(); 

        Debug.Log($"{ctx.gameObject.name} is Performing a monster tell");

        yield return new WaitForSeconds(ms.tellDuration); 
        
        ctx.agent.isStopped = false; 
        _isPerformingTell = false; 
    }

    // if action switches during coroutine
    public override void OnExit()
    {
        StopAllCoroutines();
        _isPerformingTell = false;
        ctx.agent.isStopped = false;
    }
}