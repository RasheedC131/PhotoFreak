using UnityEngine;

public class CameraAutoFocus : MonoBehaviour
{
    private bool isActive = false;

    private float focus    = 0f;
    private float maxFocus = 1f;
    private float minFocus = 0f;

    [Header("Focus Costs")]
    [SerializeField] private float moveCost      = 0.3f;  // WASD movement drain per second
    [SerializeField] private float lookCost      = 0.6f;  // Camera rotation drain per second
    [SerializeField] private float stabilizeRate = 0.7f;  // Gain per second when still

    [Header("Target Tracking")]
    [SerializeField] private float targetChangeDrop  = 0.5f;
    [SerializeField] private Camera cam;
    [SerializeField] private float  maxRaycastDistance = 50f;
    [SerializeField] private LayerMask raycastMask     = ~0;

    private bool       isMoving  = false;
    private bool       isLooking = false;
    private GameObject lastTarget = null;

    private InputManager inputManager;

    void Awake()
    {
        inputManager = transform.root.GetComponent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnMove += OnMoveInput;
            inputManager.OnLook += OnLookInput;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // If the camera has moved off the previous subject, penalise focus immediately
        GameObject currentTarget = GetCurrentTarget();
        if (currentTarget != lastTarget)
        {
            // Only penalise when switching away from a real subject (not just entering frame)
            if (lastTarget != null) focus = Mathf.Max(focus - targetChangeDrop, minFocus);

            lastTarget = currentTarget;
        }

        // focus drift (update focus ui when they snap to a different target and pic become blurry again)
        if (isLooking) focus -= lookCost * Time.deltaTime;
        
        else if (isMoving) focus -= moveCost * Time.deltaTime;
        
        else focus += stabilizeRate * Time.deltaTime;

        focus = Mathf.Clamp(focus, minFocus, maxFocus);
    }

    private void OnMoveInput(Vector2 input)
    {
        isMoving = input.sqrMagnitude > 0.01f;
    }

    private void OnLookInput(Vector2 input)
    {
        isLooking = input.sqrMagnitude > 0.01f;
    }

    // Returns the closest PhotoTag root object in the camera's line of sight, or null if nothing is in frame
    private GameObject GetCurrentTarget()
    {
        if (cam == null) return null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance, raycastMask);

        float     closest = float.MaxValue;
        GameObject result = null;

        foreach (RaycastHit hit in hits)
        {
            PhotoTag pt = hit.collider.GetComponentInParent<PhotoTag>();
            if (pt != null && hit.distance < closest)
            {
                closest = hit.distance;
                result  = pt.gameObject;
            }
        }

        return result;
    }

    private void ResetFocus()
    {
        focus      = minFocus;
        lastTarget = null;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (!isActive) ResetFocus();
    }

    public float GetFocus()
    {
        return focus;
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnMove -= OnMoveInput;
            inputManager.OnLook -= OnLookInput;
        }
    }
}
