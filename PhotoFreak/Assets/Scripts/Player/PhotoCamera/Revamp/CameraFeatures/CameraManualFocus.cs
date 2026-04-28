using UnityEngine;

public class CameraManualFocus : MonoBehaviour
{
    private bool isActive = false;

    [SerializeField] private Camera camera;
    private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float focusSpeed = 0.2f;
    [SerializeField] private float focusOffset = 0.2f;  
    [SerializeField] private float blurRandomness = 5f;
    [SerializeField] private float minFocusDist = 0.1f; 
    [SerializeField] private float maxFocusDist = 100f;
    [SerializeField] private LayerMask focusLayerMask;

    private float currFocusDist; 
    private float targetTrueDist;

    //Other Scripts
    private InputManager inputManager;


    void Awake()
    {
        inputManager = transform.root.GetComponent<InputManager>();
        
        if (inputManager != null)
        {
            inputManager.OnFocus += AdjustFocus;
        }

        cameraTransform = camera.transform;
    }

    void Update()
    {
        if (!isActive) return;

        UpdateTargetDistance(); 
    }

    private void InitializeFocus()
    {
        UpdateTargetDistance();

        float randomOffset = Random.Range(-blurRandomness, blurRandomness);
        if(Mathf.Abs(randomOffset) < 1f) randomOffset = (randomOffset > 0) ? 2f : -2f;

        currFocusDist = targetTrueDist + randomOffset;
        currFocusDist = Mathf.Clamp(currFocusDist, minFocusDist, maxFocusDist);
    }

    // adjust the focus based on the user input
    private void AdjustFocus(float scrollAmount)
    {
        if (!isActive) return;

        float direction = Mathf.Clamp(scrollAmount, -1f, 1f);
        
        float change = direction * focusSpeed;

        currFocusDist += change;
        currFocusDist = Mathf.Clamp(currFocusDist, minFocusDist, maxFocusDist);
        
        Debug.Log($"Focus: {currFocusDist} | Target: {targetTrueDist}");
    }

    private void UpdateTargetDistance()
    {
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxFocusDist, focusLayerMask, QueryTriggerInteraction.Collide))        
        {
            targetTrueDist = hit.distance + focusOffset;
        }
        else
        {
            targetTrueDist = maxFocusDist;
            //No target, want to change this because its still technically possible to focus on nothing
        }
    }

    public float GetFocusError()
    {
        return Mathf.Abs(currFocusDist-targetTrueDist);
    }


    public void SetActive(bool active)
    {
        isActive = active;

        if (isActive)
        {
            InitializeFocus();
        }
    }


    void OnDisable()
    {
        if (inputManager != null) inputManager.OnFocus -= AdjustFocus; 
    }
}
