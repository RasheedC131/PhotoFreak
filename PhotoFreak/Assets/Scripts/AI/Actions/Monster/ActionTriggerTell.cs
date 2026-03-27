using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class ActionTriggerTell : UtilityAction
{
    private AIContext ctx; 
    private bool isPreformingTell = false;

    [Header("Tell Settings")]
    [SerializeField] private float tellDuration = 2.0f;

    void Awake()
    {
        ctx = GetComponentInParent<AIContext>();
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

        yield return new WaitForSeconds(tellDuration); 
        
        ctx.currentStalkTimer = 0f; 
        ctx.agent.isStopped = false; 
        isPreformingTell = false; 
    }
}