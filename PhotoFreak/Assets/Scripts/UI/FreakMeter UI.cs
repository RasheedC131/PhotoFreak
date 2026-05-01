using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class FreakMeterUI : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float maxIntensity; 
    [SerializeField] private Volume volume;
    private Vignette vignette;
    [Header("UI")]
    [SerializeField] private GameObject meter;
    [SerializeField] private Sprite strike1;
    [SerializeField] private Sprite strike2;
    [SerializeField] private Sprite strike3;
    [SerializeField] private TMP_Text strikeText;
    [SerializeField] private int maxStrikes = 3;
    [SerializeField] private Transform Needle;
    [SerializeField] private Transform secondHand;
    [SerializeField] private Transform minuteHand;
    private void Awake()
    {
        if (!volume.profile.TryGet(out vignette))
        {
            Debug.Log("vignette effect not found");
        }
    }

    public void UpdateMeter(float value, float max)
    {
        Needle.transform.rotation = Quaternion.Euler(0, 0, 100 - value*(200/max));
        if (vignette != null)
        {
            vignette.intensity.value = (value/(float)max * maxIntensity) + .1f;
        }
    }

    public void UpdateTime(float value, float max)
    {
        minuteHand.transform.rotation = Quaternion.Euler(0, 0, -60 - (max-value)*(1/(float)2)); // 60 sec = 1/12
        secondHand.transform.rotation = Quaternion.Euler(0, 0, 0 - (max-value)*(360/60)); // 1 sec is 1/60 of 360
    }

    public void UpdateStrikes(int value)
    {
        // Sprite swap
        if (meter != null)
        {
            Image img = meter.GetComponent<Image>();
            if (img != null)
            {
                switch (value)
                {
                    case 3: img.sprite = strike3; break;
                    case 2: img.sprite = strike2; break;
                    case 1: img.sprite = strike1; break;
                }
            }
        }

        // Text display
        if (strikeText != null)
            strikeText.text = $"{value} / {maxStrikes}";
    }
}
