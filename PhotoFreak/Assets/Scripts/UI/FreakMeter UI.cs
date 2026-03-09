using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreakMeterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text freakText;
    [SerializeField] private TMP_Text timeText;

    // Update is called once per frame
    public void UpdateMeter(float value)
    {
        freakText.text = string.Format("{0}", value.ToString("F2"));
    }

    public void UpdateTime(float value)
    {
        timeText.text = string.Format("{0}", value.ToString("F2"));
    }
}
