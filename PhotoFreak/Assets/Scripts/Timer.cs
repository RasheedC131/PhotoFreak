using UnityEngine;

public class Timer : MonoBehaviour
{
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
            }
        }
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
