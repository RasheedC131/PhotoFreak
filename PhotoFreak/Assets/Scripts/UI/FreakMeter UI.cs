using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreakMeterUI : MonoBehaviour
{
    [SerializeField] private Transform Needle;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text strikeText;

    // Update is called once per frame
    public void UpdateMeter(float value, float max)
    {
        Needle.transform.rotation = Quaternion.Euler(0, 0, 100 - value*(200/max));
    }

    public void UpdateTime(float value)
    {
        timeText.text = string.Format("{0}", value.ToString("F2"));
    }

    public void UpdateStrikes(int value)
    {
        strikeText.text = string.Format("{0}", value);
    }
}
