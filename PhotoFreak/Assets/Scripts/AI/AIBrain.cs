using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIBrain : MonoBehaviour
{
    [Header("Brain Settings")]
    public float decisionInterval = 0.25f; 
    
    [Header("Debug Info")]
    public UtilityAction currentAction;
    public UtilityAction[] availableActions;

    private Coroutine brainCoroutine;
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;

    void Start()
    {
        availableActions = GetComponentsInChildren<UtilityAction>(true);
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        
        brainCoroutine = StartCoroutine(BrainTickRoutine());
    }

    private IEnumerator BrainTickRoutine()
    {
        float randomStartDelay = Random.Range(0f, decisionInterval);
        yield return new WaitForSeconds(randomStartDelay);
 
        while (true)
        {
            UtilityAction newAction = ChooseBestAction();

            if (newAction != currentAction)
            {
                if (currentAction != null) currentAction.OnExit();
                

                currentAction = newAction;

                if (obstacle != null && obstacle.enabled)
                {
                    obstacle.enabled = false;
                    yield return null; // Wait 1 frame
                }

                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                    yield return null; // Wait 1 frame
                }

                if (currentAction != null) currentAction.OnEnter();
                
            }

            if (currentAction != null) currentAction.ExecuteAction();
            else Debug.LogWarning($"{gameObject.name} has no valid action, all scores are 0");
            
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private UtilityAction ChooseBestAction()
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

        return bestAction;
    }

    void OnDisable()
    {
        if (brainCoroutine != null)
        {
            StopCoroutine(brainCoroutine);
        }
    }
}