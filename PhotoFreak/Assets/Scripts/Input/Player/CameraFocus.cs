using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 
using TMPro;
using UnityEngine.UI;

public class CameraFocus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private Volume globalVolume; 
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private TextMeshProUGUI focusIndicatorText;
    [SerializeField] private Image viewFinderImage; 

    [Header("Settings")]
    [SerializeField] private float baseFocusSpeed = 0.5f;     
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
        float allowedError = GetAllowedError(); 
        float diff = Mathf.Abs(currFocusDist - targetTrueDist);

        // normalize the score (0, 1)
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
        if (focusIndicatorText == null || viewFinderImage == null) return;

        float score = GetFocusScore(); 
        Color baseColor = viewFinderImage.color; 
        focusIndicatorText.color = baseColor; 

        if (score > 0.85)
        {
            focusIndicatorText.text = $"PERFECT [{score*100:F0}%]";
            focusIndicatorText.color = baseColor; 
        }
        else
        {
            focusIndicatorText.text = $"FOCUSING... [{score*100:F0}%]";
            focusIndicatorText.color = baseColor; 
        }
    }
}