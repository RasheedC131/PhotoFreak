using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private DepthOfField dof;

    [Header("Blur Settings")]
    [SerializeField] private float focalLength = 50f;      // Lens focal length in mm (keep at 50)
    [SerializeField] private float minAperture = 1.4f;     // Max blur — unfocused
    [SerializeField] private float maxAperture = 16f;      // Min blur — fully focused

    //Other Scripts
    [SerializeField] private CameraController controller;
    [SerializeField] private CameraAutoFocus autoFocus;
    [SerializeField] private CameraManualFocus manualFocus;

    void Awake()
    {
        if (globalVolume == null)
        {
            Debug.LogError("[BlurController] Global Volume not assigned!");
            return;
        }

        if (globalVolume.profile == null)
        {
            Debug.LogError("[BlurController] Volume has no Profile assigned!");
            return;
        }

        VolumeProfile profile = globalVolume.profile;

        if (!profile.TryGet(out dof))
        {
            dof = profile.Add<DepthOfField>(true);
            Debug.Log("[BlurController] DepthOfField not found in profile — added at runtime.");
        }

        dof.mode.value            = DepthOfFieldMode.Bokeh;
        dof.mode.overrideState    = true;
        dof.focalLength.overrideState  = true;
        dof.aperture.overrideState     = true;
        dof.focusDistance.overrideState = true;

        dof.focalLength.value   = focalLength;
        dof.aperture.value      = maxAperture;
        dof.focusDistance.value = 5f;
        dof.active = false;
    }

    void Update()
    {
        if (controller == null || dof == null) return;

        bool aiming = controller.getCameraState();

        if (dof.active != aiming)
        {
            dof.active = aiming;
            if (autoFocus != null) autoFocus.SetActive(aiming);
        }

        if (aiming)
            ApplyBlur();
    }

    private void ApplyBlur()
    {
        float focusValue;

        if (controller.currentCamera.manualFocus)
        {
            float error = manualFocus != null ? manualFocus.GetFocusError() : 0f;
            focusValue = Mathf.Clamp01(1f - (error / 10f));
        }
        else
        {
            focusValue = autoFocus != null ? autoFocus.GetFocus() : 1f;
        }

        // Low aperture = wide open = lots of blur; high aperture = narrow = sharp
        dof.aperture.value = Mathf.Lerp(minAperture, maxAperture, focusValue);
    }
}
