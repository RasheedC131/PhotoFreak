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

    // Full unfiltered set — rebuilt once in Start, never changes at runtime.
    private UtilityAction[] _allActions;
    private AIContext _ctx;
    private Coroutine brainCoroutine;
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;

    void Start()
    {
        _ctx        = GetComponent<AIContext>();
        agent       = GetComponent<NavMeshAgent>();
        obstacle    = GetComponent<NavMeshObstacle>();

        _allActions = GetComponentsInChildren<UtilityAction>(true);
        RefreshAvailableActions();

        brainCoroutine = StartCoroutine(BrainTickRoutine());
    }

    // Call this whenever ctx.isMonster changes (e.g. from MatchManager after
    // infecting an NPC) so the brain immediately switches to the correct action set.
    public void RefreshAvailableActions()
    {
        if (_ctx == null || _allActions == null) return;

        bool isMonster = _ctx.isMonster;
        int count = 0;
        foreach (var a in _allActions)
            if (a.isMonsterAction == isMonster) count++;

        availableActions = new UtilityAction[count];
        int idx = 0;
        foreach (var a in _allActions)
            if (a.isMonsterAction == isMonster) availableActions[idx++] = a;
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

                // If this NPC was parked as an obstacle, tear down that state before
                // re-enabling the agent. The one-frame gaps let the NavMesh bake update.
                if (obstacle != null && obstacle.enabled)
                {
                    obstacle.enabled = false;
                    yield return null;
                }

                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                    yield return null;
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