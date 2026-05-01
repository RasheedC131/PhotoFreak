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

        VolumeProfile profile = globalVolume.profile;

        if (!profile.TryGet(out dof))
        {
            dof = profile.Add<DepthOfField>(true);
            Debug.Log("[BlurController] DepthOfField not found in profile — added at runtime.");
        }

        dof.mode.value = DepthOfFieldMode.Bokeh;
        dof.mode.overrideState = true;
        dof.focusDistance.overrideState = true;
        dof.aperture.overrideState = true;
        dof.focalLength.overrideState = true;
        dof.active = true;
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
    }
}