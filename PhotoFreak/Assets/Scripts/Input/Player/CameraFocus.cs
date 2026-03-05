using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 

public class CameraFocus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private Volume globalVolume; 
    [SerializeField] private Transform cameraTransform; 

    [Header("Settings")]
    [SerializeField] private float baseFocusSpeed = 0.5f;     
    [SerializeField] private float aperture = 5.6f; 
    [SerializeField] private float blurRandomness = 5f; 
    [SerializeField] private float focusTolerance = 0.25f;       
    [SerializeField] private float minFocusDist = 0.1f; 
    [SerializeField] private float maxFocusDist = 100f; 

    private DepthOfField dof; 
    private float currFocusDist; 
    private float targetTrueDist; 

    void Awake()
    {
        if (globalVolume.profile.TryGet(out DepthOfField tmpDof))
        {
            dof = tmpDof; 
        }

        if (inputManager == null) inputManager = FindAnyObjectByType<InputManager>(); 
    }

    void OnEnable()
    {
        if (inputManager != null) inputManager.OnScroll += AdjustFocus; 
        InitializeFocus();
    }

    void OnDisable()
    {
        if (inputManager != null) inputManager.OnScroll -= AdjustFocus; 
    }

    void Update()
    {
        UpdateTargetDistance(); 
        CheckFocusQuality(); 
    }

    private void InitializeFocus()
    {
        UpdateTargetDistance(); 
     
        float randomOffset = Random.Range(-blurRandomness, blurRandomness);
        if(Mathf.Abs(randomOffset) < 1f) randomOffset = (randomOffset > 0) ? 2f : -2f;

        currFocusDist = targetTrueDist + randomOffset;
        currFocusDist = Mathf.Clamp(currFocusDist, minFocusDist, maxFocusDist);

        UpdateDoF();
    }

    // uses a log scaling to take adjust the focus
    private void AdjustFocus(float scrollAmount)
    {
        float direction = Mathf.Clamp(scrollAmount, -1f, 1f);
        
        float dynamicSpeed = currFocusDist * baseFocusSpeed; 
        dynamicSpeed = Mathf.Max(dynamicSpeed, 0.1f); 

        // calc the focus dist and normalize the value 
        currFocusDist += direction * dynamicSpeed;
        currFocusDist = Mathf.Clamp(currFocusDist, minFocusDist, maxFocusDist); 

        UpdateDoF();
    }

    private void UpdateDoF()
    {
        if (dof != null) dof.focusDistance.value = currFocusDist;
    }

    private float GetAllowedError()
    {
        float tolerancePercentage = aperture / 100f; 
        float errorMargin = targetTrueDist * tolerancePercentage;

        return Mathf.Max(errorMargin, focusTolerance);
    }

    private void CheckFocusQuality()
    {
        if (Mathf.Abs(currFocusDist - targetTrueDist) < GetAllowedError())
        {
            // TODO: add logic for successfully snapping a photo 
        }
    }

    public float GetFocusScore()
    {
        // calc margin based on the aperture for the score 
        float tolerancePercentage = aperture / 100f; 
        float allowedError = targetTrueDist * tolerancePercentage;
        
        // buffer to allow for close up shots 
        allowedError = Mathf.Max(allowedError, 0.2f); 

        float diff = Mathf.Abs(currFocusDist - targetTrueDist);

        if (diff > allowedError) return 0f; 
        
        // calc score from 0 to 1 
        return 1f - (diff / allowedError);
    }

    private void UpdateTargetDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxFocusDist))
        {
            targetTrueDist = hit.distance;
        }
        else
        {
            targetTrueDist = maxFocusDist; 
        }
    }

    void OnGUI()
    {
        float centerX = Screen.width / 2 - 100; 
        float centerY = Screen.height / 2 + 50; 

        GUI.color = Color.red; 
        GUI.Label(new Rect(centerX, centerY, 300, 20), $"Target: {targetTrueDist:F2}");
        GUI.Label(new Rect(centerX, centerY + 20, 300, 20), $"Current: {currFocusDist:F2}");
        
        float diff = Mathf.Abs(currFocusDist - targetTrueDist);
        string status = (diff < GetAllowedError()) ? "PERFECT" : "BLURRY";
        
        GUI.Label(new Rect(centerX, centerY + 40, 300, 20), status);
    }
}