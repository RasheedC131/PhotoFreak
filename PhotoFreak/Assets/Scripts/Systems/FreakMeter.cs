using UnityEngine;
using UnityEditor;
using System.Collections.Generic; 
using UnityEngine.SceneManagement;
using System.Collections;

public class FreakMeter : MonoBehaviour
{
    [Header("FreakMeter settings")]
    [SerializeField] private int maxNPC;
    [SerializeField] private float maxFreak;
    [SerializeField] private int maxStrikes;
    [SerializeField] private int decayRate;
    [Header("Camera Freak Function")]
    [SerializeField] private float k1;
    [SerializeField] private float x1;
    [Header("Sprint Freak Function")]
    [SerializeField] private float k2;
    [SerializeField] private float x2;
    [Header("Script info")]
    [SerializeField] PhotoCamera CameraScript;
    [SerializeField] PlayerMovement player;
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private Timer timer;
    [SerializeField] private FreakMeterTimer freakTimer;

    private float currentFreak;
    private bool isMeterDecaying;
    private int count;
    private List<Transform> visibleNPCs = new List<Transform>();
    
    private bool isGameOver = false; 
    private float prevVal = 0;
    private int currentStrikes = 0;
    private bool striked = false;
    void Start()
    {
        count = 0;

        UpdateUI(); 
        currentFreak = 0f; 

        if (UI == null) Debug.LogError("[]FreakMeter]: UI reference is missing in the Inspector!");
        if (CameraScript == null) Debug.LogError("[]FreakMeter]: CameraScript reference is missing!");
        UI.UpdateStrikes(currentStrikes);
    }

    void Update()
    {
        if (isGameOver) return;

        if (count > maxNPC) count = maxNPC;

        if (currentFreak >= maxFreak)
        {
            if (!striked)
                striked = true;
            freakTimer.restartTime();
            currentStrikes += 1;
            if (currentStrikes >= maxStrikes)
            {
                TriggerGameOver(); 
                return; 
            }
            currentFreak = maxFreak - 2;
            if (UI != null) UI.UpdateMeter(currentFreak, maxFreak);
            UI.UpdateStrikes(currentStrikes);
        }

        if (striked)
        {
            currentFreak -= .01f * 10;
            UI.UpdateMeter(currentFreak, maxFreak);
            if (currentFreak <= 0)
            {
                currentFreak = 0;
                striked = false;
            }
        }

        bool isMeterRising = false; 

        if (player.getSprint() && count > -1 && !striked)
        {
            Time.timeScale = 50;
            freakTimer.unpause();
            timer.restart();
            currentFreak += sprintFunction(1, freakTimer.getTime()) * 1f; // tmp values
            isMeterRising = true; 
        }
        else if (CameraScript.getCameraState() && !striked)
        {
            freakTimer.unpause();
            timer.restart();
            currentFreak += cameraFunction(count, freakTimer.getTime()) * 1f;
            isMeterRising = true; 
        }
        else
        {
            freakTimer.pause();
        }
        
        
        if (isMeterRising)
        {
            UpdateUI();
        }
        else if (isMeterDecaying)
        {
            freakTimer.pause();
            if (currentFreak > 0)
            {
                currentFreak -= .01f * decayRate;
                if (currentFreak < 0)
                {
                    currentFreak = 0;
                    isMeterDecaying = false;
                }
                freakTimer.restartTime();
                UI.UpdateMeter(currentFreak, maxFreak);
            }
        }
        if (timer.getTime() <= 0)
        {
            isMeterDecaying = true;
        }
        else
        {
            isMeterDecaying = false;
        }
    }

    float sprintFunction(int count, float time)
    {
        float val = k2 * Mathf.Pow(x2, time);
        prevVal = val  * count - prevVal;
        if (prevVal < 0)
        {
            prevVal = 0;
        }
        return prevVal;
    }
    float cameraFunction(int count, float time)
    {
        float val = k1 * Mathf.Pow(x1, time);
        prevVal = val * count - prevVal;
         if (prevVal < 0)
        {
            prevVal = 0;
        }
        return prevVal;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Guest") || other.CompareTag("Monster")) 
        {
            if (!visibleNPCs.Contains(other.transform))
            {
                visibleNPCs.Add(other.transform);
                count = visibleNPCs.Count;
                Debug.Log("NPC Entered Range, Count: " + count);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Guest") || other.CompareTag("Monster"))
        {
            if (visibleNPCs.Contains(other.transform))
            {
                visibleNPCs.Remove(other.transform);
                count = visibleNPCs.Count;
                Debug.Log("NPC Left Range, Count: " + count);
            }
        }
    }

    public void AddFreakScore(float amount)
    {
        if (isGameOver) return; 

        currentFreak += amount; 

        if (currentFreak > maxFreak) currentFreak = maxFreak; 
        UpdateUI(); 
    }

    private void UpdateUI()
    {
        UI.UpdateMeter(currentFreak, maxFreak);
        if(timer != null) timer.restart();
        isMeterDecaying = true;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;

        Debug.Log("GAME OVER: Too much freakiness!"); 
        
        Time.timeScale = 1f; 

        StartCoroutine(RestartGameRoutine());
    }

    private IEnumerator RestartGameRoutine()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}