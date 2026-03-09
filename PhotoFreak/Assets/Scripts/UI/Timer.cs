using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;
public class Timer : MonoBehaviour
{
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private GameObject player;
    [SerializeField] private bool MainTimer;
    public float timeRemaining = 10;
    private float currentTime;
    private bool tmp;

    void Start()
    {
        tmp = true;
        currentTime = timeRemaining;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime >= 0)
        {
            currentTime -= Time.deltaTime;
            // Debug.Log(currentTime);

        }
        else
        {
            if (tmp)
            {
                Debug.Log("Timeout");
                tmp = false;
                if (MainTimer) // deletes player
                {
                    StartCoroutine(RestartGameRoutine());                
                }
            }
        }
        if (UI != null)
            UI.UpdateTime(currentTime);
    }

    public void restart()
    {
        currentTime = timeRemaining;
        tmp = true;
    }
    public float getTime()
    {
        return currentTime;
    }

    private IEnumerator RestartGameRoutine()
    {
        Debug.Log("Time's Up! Restarting...");
        if (player != null) player.SetActive(false); 
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
