using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private bool isActive = false;

    [Header("Camera")]
    [SerializeField] private Camera camera;
    private float defaultFOV;

    [Header("Settings")]
    [SerializeField] private float zoomSpeed = 50f;
    [SerializeField] private float maxZoom = 3f; 
    [SerializeField] private float minZoom = 1f; 
    public float currZoomLevel = 1f;

    //Other Scripts
    private InputManager inputManager;

    
    void Awake()
    {
        inputManager = transform.root.GetComponent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnZoom += AdjustZoom;
        }

        if (camera == null) camera = transform.root.GetComponent<Camera>();
        defaultFOV = camera.fieldOfView;
    }

    private void AdjustZoom(float scrollAmount)
    {
        if (!isActive) return;

        float direction = Mathf.Clamp(scrollAmount, -1f, 1f); //zoom in or out
        currZoomLevel += direction * zoomSpeed * Time.deltaTime;  //zoom amount
        currZoomLevel = Mathf.Clamp(currZoomLevel, minZoom, maxZoom); //capping zoom amount

        ApplyZoom();
    }

    private void ApplyZoom()
    {
        camera.fieldOfView = defaultFOV / currZoomLevel;
    }

    private void ResetZoom()
    {
        currZoomLevel = 1f;
        ApplyZoom();
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (!isActive)
        {
            ResetZoom();
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnZoom -= AdjustZoom;
        }
    }
}
