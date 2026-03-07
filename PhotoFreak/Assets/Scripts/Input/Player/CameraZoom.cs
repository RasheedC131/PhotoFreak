using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 
using TMPro; 

public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private Camera photoCamera; 
    [SerializeField] private Volume globalVolume; 
    [SerializeField] private TextMeshProUGUI zoomIndicatorText; 
    [Header("Settings")]
    [SerializeField] private float maxZoom = 3f; 
    [SerializeField] private float minZoom = 1f; 
    [SerializeField] private float zoomSpeed = 50f; 

    public float currZoomLevel {get; private set; } = 1f; 
    private float defaultFOV; 
    private float defaultFocalLength = 50f; 
    private DepthOfField dof; 

    void Awake()
    {
        if (inputManager == null) inputManager = FindAnyObjectByType<InputManager>(); 
        if (photoCamera == null) photoCamera = GetComponent<Camera>(); 
        
        defaultFOV = photoCamera.fieldOfView; 

        // set our focal length to the one found in global volume 
        if (globalVolume.profile.TryGet(out DepthOfField tmpDof))
        {
            dof = tmpDof; 
            if (dof.focalLength.overrideState) defaultFocalLength = dof.focalLength.value; 
        }
    }

    void OnEnable()
    {
        if (inputManager != null) inputManager.OnZoom += AdjustZoom;        
        ResetZoom(); 
    }

    void OnDisable()
    {
        if (inputManager != null) inputManager.OnZoom -= AdjustZoom; 
    }

    private void AdjustZoom(float scrollAmount)
    {
        float direction = Mathf.Clamp(scrollAmount, -1f, 1f); 
        currZoomLevel += direction * zoomSpeed * Time.deltaTime; 
        currZoomLevel = Mathf.Clamp(currZoomLevel, minZoom, maxZoom); 

        ApplyZoomPhysics();
    }

    private void ApplyZoomPhysics()
    {
        if (photoCamera != null) photoCamera.fieldOfView = defaultFOV / currZoomLevel; 

        if (dof != null) dof.focalLength.value = defaultFocalLength * currZoomLevel;

        // draw the zoom level onto the ui 
        if (zoomIndicatorText != null) zoomIndicatorText.text = $"{currZoomLevel:F1}x";
    }

    public void ResetZoom()
    {
        currZoomLevel = 1f; 
        ApplyZoomPhysics();
    }
}
