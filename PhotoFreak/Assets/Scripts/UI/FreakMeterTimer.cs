using UnityEngine;

public class FreakMeterTimer : MonoBehaviour
{
    private float time = 0;
    // Update is called once per frame
    void Update()
    {
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
}
