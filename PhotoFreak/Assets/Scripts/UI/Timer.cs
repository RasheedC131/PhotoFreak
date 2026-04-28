using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class Timer : MonoBehaviour
{
    // The main game countdown timer — accessible globally so AI considerations can read TimeRatio.
    public static Timer MainInstance { get; private set; }

    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private GameObject player;
    [SerializeField] private bool MainTimer;
    public float timeRemaining = 10f;
    private float currentTime;
    private bool isTimerFinished;

    /// <summary>Normalised time remaining: 1.0 = full time, 0.0 = expired.</summary>
    public float TimeRatio => timeRemaining > 0f ? Mathf.Clamp01(currentTime / timeRemaining) : 0f;

    void Start()
    {
        if (MainTimer) MainInstance = this;
        restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimerFinished) return;

        currentTime -= Time.deltaTime; 

        if (UI != null) UI.UpdateTime(Mathf.Max(0, currentTime), timeRemaining); 

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
