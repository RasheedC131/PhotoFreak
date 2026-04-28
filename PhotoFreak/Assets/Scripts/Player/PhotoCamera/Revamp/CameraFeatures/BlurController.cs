using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private DepthOfField dof;

    [Header("Blur Settings")]
    [SerializeField] private float maxBlur = 10f;

    //Other Scripts
    private CameraController controller;
    private CameraAutoFocus autoFocus;
    private CameraManualFocus manualFocus;

    void Awake()
    {
        controller = GetComponentInParent<CameraController>();
        autoFocus = GetComponent<CameraAutoFocus>();
        manualFocus = GetComponent<CameraManualFocus>();

        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not assigned!");
           return;
        }

        if (globalVolume.profile == null)
        {
            Debug.LogError("Volume has no Profile assigned!");
            return;
        }

        if (globalVolume.profile.TryGet(out DepthOfField tmp))
        {
            dof = tmp;
            dof.focusDistance.overrideState = true;
            dof.aperture.overrideState = true;
            dof.active = true;
        }
        else
        {
            Debug.LogError("No Depth Of Field override found in Volume Profile!");
        }
    }

    void Update()
    {
        if(!controller.getCameraState()) return;
        ApplyBlur();
    }

    private void ApplyBlur()
    {
        if (dof == null)
        {
           return; 
        }

        float focusValue;

        if (controller.currentCamera.manualFocus)
        {
            float error = manualFocus.GetFocusError();

            focusValue = Mathf.Clamp01(1f - (error / 10f));
        }
        else
        {
            focusValue = autoFocus.GetFocus();
        }

        float blurAmount = Mathf.Lerp(maxBlur, 0f, focusValue);

        dof.focalLength.value = blurAmount;

        Debug.Log("boom");
    }
}