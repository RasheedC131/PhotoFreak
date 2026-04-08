using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;
public class Timer : MonoBehaviour
{
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private GameObject player;
    [SerializeField] private bool MainTimer;
    public float timeRemaining = 10f;
    private float currentTime;
    private bool isTimerFinished;

    void Start()
    {
        restart(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimerFinished) return;

        currentTime -= Time.deltaTime; 

        if (UI != null) UI.UpdateTime(Mathf.Max(0, currentTime)); 

        if (currentTime <= 0) HandleTimeOut(); 

    }

    public void restart()
    {
        currentTime = timeRemaining;
        isTimerFinished = false;
    }

    public float getTime()
    {
        return currentTime;
    }

    private void HandleTimeOut()
    {
        isTimerFinished = true; 
        currentTime = 0f; 

        if (MainTimer && GlobalGameState.Instance != null)
        { 
            GlobalGameState.Instance.TriggerGameOver(); 
        }

    }
}
