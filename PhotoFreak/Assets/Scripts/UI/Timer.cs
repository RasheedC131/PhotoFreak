using UnityEngine;

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
                    Destroy(player);
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
}
