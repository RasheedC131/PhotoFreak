using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBlurController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private DepthOfField dof;
    private VolumeProfile runtimeProfile;

    [Header("Blur Settings")]
    [SerializeField] private float focalLength = 100f;
    [SerializeField] private float aperture = 1.4f;

    [Header("Focus Distance")]
    [SerializeField] private float unfocusedDistance  = 0.5f;
    [SerializeField] private float maxRaycastDistance = 50f;
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float defaultFocusDistance = 5f;

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private CameraController  controller;
    [SerializeField] private CameraAutoFocus   autoFocus;
    [SerializeField] private CameraManualFocus manualFocus;

    private bool wasAiming = false;

    // Large value pushes the focus plane far beyond the scene — no visible blur
    private const float IdleFocusDistance = 500f;

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("[BlurController] Global Volume not assigned!");
            return;
        }

        runtimeProfile = globalVolume.profile;

        if (runtimeProfile == null)
        {
            Debug.LogError("[BlurController] Volume Profile missing!");
            return;
        }

        if (!runtimeProfile.TryGet(out dof))
        {
            dof = runtimeProfile.Add<DepthOfField>(true);
            Debug.Log("[BlurController] DepthOfField added to runtime profile.");
        }

        dof.active                      = true;
        dof.mode.value                  = DepthOfFieldMode.Bokeh;
        dof.mode.overrideState          = true;
        dof.focalLength.overrideState   = true;
        dof.aperture.overrideState      = true;
        dof.focusDistance.overrideState = true;

        dof.focalLength.value   = focalLength;
        dof.aperture.value      = aperture;
        dof.focusDistance.value = IdleFocusDistance;
    }

    void Update()
    {
        if (controller == null || dof == null) return;

        bool aiming = controller.getCameraState();

        if (aiming != wasAiming)
        {
            wasAiming = aiming;
            if (autoFocus != null) autoFocus.SetActive(aiming);
        }

        if (aiming)
        {
            ApplyFocus();
        }
        else
        {
            // No blur when not aiming — focus plane is 500m away
            dof.focusDistance.value = IdleFocusDistance;
        }
    }

    private void ApplyFocus()
    {
        // Get focus quality (0 = unfocused/shaky, 1 = locked on)
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

        // Find distance to subject
        float subjectDistance = GetSubjectDistance();

        // At focusValue=0: focus plane is at unfocusedDistance (subject blurry)
        // At focusValue=1: focus plane is exactly at subject (subject sharp, bg blurry)
        dof.focusDistance.value = Mathf.Lerp(unfocusedDistance, subjectDistance, focusValue);
        dof.aperture.value      = aperture;
        dof.focalLength.value   = focalLength;
    }

    private float GetSubjectDistance()
    {
        if (cam == null) return defaultFocusDistance;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance, raycastMask);

        float closest = -1f;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<PhotoTag>() != null)
            {
                if (closest < 0f || hit.distance < closest)
                    closest = hit.distance;
            }
        }

        return closest > 0f ? Mathf.Max(closest, 0.1f) : defaultFocusDistance;
    }
}
