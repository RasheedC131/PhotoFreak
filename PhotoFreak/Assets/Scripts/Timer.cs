using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    private float currentTime;

    void Start()
    {
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
            Debug.Log("Switch");
            currentTime = timeRemaining;
        }
    }

    public float getTime()
    {
        return currentTime;
    }
}
