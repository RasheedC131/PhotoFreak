using UnityEngine;

public class FreakMeterTimer : MonoBehaviour
{
    private bool paused = true;
    private float time = 0;
    // Update is called once per frame
    void Update()
    {
        if (!paused)
            time += Time.deltaTime;
    }

    public float getTime()
    {
        return time;
    }

    public void restartTime()
    {
        time = 0;
    }

    public void pause()
    {
        paused = true;
    }
    public void unpause()
    {
        paused = false;
    }
}
