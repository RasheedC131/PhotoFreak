using System.Collections;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [Header("Brain Settings")]
    public float decisionInterval = 0.25f; 
    
    [Header("Debug Info")]
    public UtilityAction currentAction;
    public UtilityAction[] availableActions;

    private Coroutine brainCoroutine;

    void Start()
    {
        availableActions = GetComponentsInChildren<UtilityAction>();
        brainCoroutine = StartCoroutine(BrainTickRoutine());
    }

    private IEnumerator BrainTickRoutine()
    {

        float randomStartDelay = Random.Range(0f, decisionInterval);
        yield return new WaitForSeconds(randomStartDelay);
 
        while (true)
        {
            ChooseBestAction();

        if (currentAction != null)
        {
            Debug.Log($"{gameObject.name} chose to execute {currentAction.gameObject.name}. Agent on NavMesh: {GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh}");
            currentAction.ExecuteAction();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no valid action, all scores are 0");
        }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private void ChooseBestAction()
    {
        float highestScore = 0.01f;
        UtilityAction bestAction = null;

        foreach (UtilityAction action in availableActions)
        {
            if (!action.gameObject.activeInHierarchy) continue;

            float score = action.CalculateUtilityScore();

            if (score > highestScore)
            {
                highestScore = score;
                bestAction = action;
            }
        }

        if (bestAction is not null) currentAction = bestAction;
        
    }

    void OnDisable()
    {
        if (brainCoroutine != null)
        {
            StopCoroutine(brainCoroutine);
        }
    }
}