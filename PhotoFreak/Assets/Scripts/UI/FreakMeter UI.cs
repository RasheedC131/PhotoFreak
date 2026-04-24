using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreakMeterUI : MonoBehaviour
{
    [SerializeField] private Transform Needle;
    [SerializeField] private Transform secondHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private TMP_Text strikeText;

    // Update is called once per frame
    public void UpdateMeter(float value, float max)
    {
        Needle.transform.rotation = Quaternion.Euler(0, 0, 100 - value*(200/max));
    }

    public void UpdateTime(float value, float max)
    {
        minuteHand.transform.rotation = Quaternion.Euler(0, 0, -60 - (max-value)*(1/(float)2)); // 60 sec = 1/12
        secondHand.transform.rotation = Quaternion.Euler(0, 0, 0 - (max-value)*(360/60)); // 1 sec is 1/60 of 360

    }

    public void UpdateStrikes(int value)
    {
        if (value == 0)
            strikeText.text = "";
        else
        {
            strikeText.text = strikeText.text + "X";
        }   
    }
}
