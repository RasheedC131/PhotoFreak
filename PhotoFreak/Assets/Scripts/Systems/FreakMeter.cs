using UnityEngine;
using UnityEditor;
using System.Collections.Generic; 
using UnityEngine.SceneManagement;
using System.Collections;

public class FreakMeter : MonoBehaviour
{
    [SerializeField] PhotoCamera CameraScript;
    [SerializeField] PlayerMovement player;
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private Timer timer;

    [SerializeField] private int maxNPC;
    [SerializeField] private float maxFreak;
    [SerializeField] private int maxStrikes;

    private float currentFreak;
    private bool isMeterDecaying;
    private int count;
    private List<Transform> visibleNPCs = new List<Transform>();
    
    private bool isGameOver = false; 

    void Start()
    {
        count = 0;

        UpdateUI(); 
        currentFreak = 0f; 

        if (UI == null) Debug.LogError("[]FreakMeter]: UI reference is missing in the Inspector!");
        if (CameraScript == null) Debug.LogError("[]FreakMeter]: CameraScript reference is missing!");
    }

    void Update()
    {
        if (isGameOver) return; 

        if (count > maxNPC) count = maxNPC;

        if (currentFreak >= maxFreak)
        {
            maxStrikes -= 1;
            currentFreak = 0;
            if (UI != null) UI.UpdateMeter(currentFreak);
        }
        
        if (maxStrikes <= 0)
        {
            TriggerGameOver(); 
            return; 
        }

        bool isMeterRising = false; 

        if (player.getSprint() && count > 0)
        {
            currentFreak += count * .01f;
            isMeterRising = true; 
        }

        if (CameraScript.getCameraState())
        {
            currentFreak += count* .03f;
            isMeterRising = true; 
        }

        if (isMeterRising)
        {
            UpdateUI();
            isMeterDecaying = true; 
        }
        else if (timer.getTime() <= 0 && isMeterDecaying)
        {
            if (currentFreak > 0)
            {
                currentFreak -= .01f;
                if (currentFreak < 0)
                {
                    currentFreak = 0;
                    isMeterDecaying = false;
                }
                Debug.Log(currentFreak);
                UI.UpdateMeter(currentFreak);
            }
        }
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
        UI.UpdateMeter(currentFreak);
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