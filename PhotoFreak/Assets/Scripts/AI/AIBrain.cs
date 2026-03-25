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
                currentAction.ExecuteAction();
            }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private void ChooseBestAction()
    {
        float highestScore = 0f;
        UtilityAction bestAction = null;

        foreach (UtilityAction action in availableActions)
        {
            float score = action.CalculateUtilityScore();

            if (score > highestScore)
            {
                highestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != null && bestAction != currentAction)
        {
            currentAction = bestAction;
        }
    }

    void OnDisable()
    {
        if (brainCoroutine != null)
        {
            StopCoroutine(brainCoroutine);
        }
    }
}