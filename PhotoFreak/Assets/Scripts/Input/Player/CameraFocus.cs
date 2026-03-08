using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 
using TMPro;
using UnityEngine.UI;

public class CameraFocus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private CameraZoom cameraZoom; 
    [SerializeField] private Volume globalVolume; 
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private TextMeshProUGUI focusIndicatorText;
    [SerializeField] private Image viewFinderImage; 
    [SerializeField] private Image focusStatusIndicator;

    [Header("Settings")]
    [SerializeField] private float baseFocusSpeed = 0.2f;     
    [SerializeField] private float aperture = 5.6f; 
    [SerializeField] private float blurRandomness = 5f; 
    [SerializeField] private float focusTolerance = 0.25f;       
    [SerializeField] private float minFocusDist = 0.1f; 
    [SerializeField] private float maxFocusDist = 100f; 

    [Header("Scoring")]
    [Range(0.1f, 10f)]
    public float scoreCurveFlatness = 4.0f;         // used for the formula to calculate the score 

    private DepthOfField dof; 
    private float currFocusDist; 
    private float targetTrueDist; 

    void Awake()
    {
        if (inputManager == null) inputManager = FindAnyObjectByType<InputManager>(); 
        
        if (cameraZoom == null) cameraZoom = GetComponent<CameraZoom>(); 
        
        if (globalVolume != null && globalVolume.profile.TryGet(out DepthOfField tmpDof)) 
        {
            dof = tmpDof;
            dof.focusDistance.overrideState = true; 
        }

        DisableDepthOfField();
    }

    void OnEnable()
    {
        if (inputManager != null) inputManager.OnFocus += AdjustFocus; 
        InitializeFocus();
    }

    void OnDisable()
    {
        if (inputManager != null) inputManager.OnFocus -= AdjustFocus; 
    }

    void Update()
    {
        UpdateTargetDistance(); 
        UpdateFocusUI();
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

    // adjust the focus based on the user input and zoom level 
    private void AdjustFocus(float scrollAmount)
    {
        float direction = Mathf.Clamp(scrollAmount, -1f, 1f);

        float zoomLevel = (cameraZoom != null) ? cameraZoom.currZoomLevel : 1f; 
        float dampenZoom = 1f / zoomLevel; 
        
        float dynamicSpeed = baseFocusSpeed * dampenZoom; 
        dynamicSpeed = Mathf.Max(dynamicSpeed, 0.1f); 
        
        float distanceMultiplier = Mathf.Max(currFocusDist, 1f);
        
        float change = direction * dynamicSpeed * distanceMultiplier;

        currFocusDist += change;
        currFocusDist = Mathf.Clamp(currFocusDist, minFocusDist, maxFocusDist); 

        Debug.Log($"Focus Dist: {currFocusDist:F2}; Target: {targetTrueDist:F2}");

        UpdateDoF();
    }

    private void UpdateDoF()
    {
        if (dof != null) dof.focusDistance.value = currFocusDist;
    }

    // private void CheckFocusQuality()
    // {
    //     if (Mathf.Abs(currFocusDist - targetTrueDist) < GetAllowedError())
    //     {
    //         // TODO: add logic for successfully snapping a photo 
    //     }
    // }

    public float GetFocusScore()
    {
        float zoomLevel = (GetComponent<Camera>() != null) ? cameraZoom.currZoomLevel : 1f; // 1f is the minimum zoom level 
        
        float tolerancePerct = aperture / 100f;
        float allowedError = targetTrueDist * tolerancePerct; 

        // need to have some margin of error so it's easier to take a decent pic 
        allowedError /= zoomLevel; 
        allowedError = Mathf.Max(allowedError, focusTolerance); 

        float diff = Mathf.Abs(currFocusDist - targetTrueDist); 
        float deviation = diff / allowedError; 

        // Apply Gaussian Bell Curve Formula: e^(-(x^2) / flatness)
        float score = Mathf.Exp(-(deviation * deviation) / scoreCurveFlatness);

        return score;
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

    private void UpdateFocusUI()
    {
        Color colorGreen = Color.green;
        Color colorOrange = new Color(1f, 0.64f, 0f); // Orange
        Color colorRed = Color.red;

        if (focusIndicatorText == null) return;

        float score = GetFocusScore(); 
        
        focusIndicatorText.text = $"FOCUS: {score*100:F0}%";
        
        if (focusStatusIndicator != null)
        {
            if (score > 0.90f)
            {
                focusStatusIndicator.color = colorGreen; 
            }
            else if (score > 0.30f)
            {
                focusStatusIndicator.color = colorOrange; 
            }
            else
            {
                focusStatusIndicator.color = colorRed; 
            }
        }
    }

    public void EnableDepthOfField()
    {
        if (dof != null) dof.active = true;
    }

    public void DisableDepthOfField()
    {
        if (dof != null) dof.active = false;
    }
}