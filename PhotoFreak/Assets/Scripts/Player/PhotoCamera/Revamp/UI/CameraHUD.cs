using UnityEngine;
using UnityEngine.UI;

public class CameraHUD : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI filmText;
    [SerializeField] private TMPro.TextMeshProUGUI zoomText;
    [SerializeField] private TMPro.TextMeshProUGUI focusText;
    [SerializeField] private Image focusIndicator;

    //Other Scripts
    private CameraController controller;
    private CameraZoom zoom;
    private CameraAutoFocus autoFocus;
    private CameraManualFocus manualFocus;

    void Awake()
    {
        controller = transform.root.GetComponentInChildren<CameraController>();
        zoom = transform.root.GetComponentInChildren<CameraZoom>();
        autoFocus = transform.root.GetComponentInChildren<CameraAutoFocus>();
        manualFocus = transform.root.GetComponentInChildren<CameraManualFocus>();
    }

    void Update()
    {
        if (!controller.getCameraState()) return;

        UpdateFilm();
        UpdateZoom();
        UpdateFocus();
    }

    private void UpdateFilm()
    {
        filmText.text = controller.getCurrFilm() + " Film";
    }

    private void UpdateZoom()
    {
        zoomText.text = $"{zoom.currZoomLevel:F1}x";
    }

    private void UpdateFocus()
    {
        float value;
        bool manual = controller.currentCamera.manualFocus;

        if (manual)
        {
            float error = manualFocus.GetFocusError();

            value = Mathf.Clamp01(1f - (error / 10f));
        }
        else
        {
            value = autoFocus.GetFocus();
        }

        focusText.text = $"FOCUS: {value * 100f:0}%";

        UpdateIndicator(value);
    }

    private void UpdateIndicator(float value)
    {
        if (focusIndicator == null) return;

        if (value > 0.8f)
            focusIndicator.color = Color.green;
        else if (value > 0.4f)
            focusIndicator.color = new Color(1f, 0.6f, 0f); // orange
        else
            focusIndicator.color = Color.red;
    }    
}
