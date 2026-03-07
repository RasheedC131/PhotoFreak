using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI; 
using System.Collections; 
using TMPro; 


public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject photoCam;
    [SerializeField] private GameObject viewFinderUI; 
    [SerializeField] private CameraFocus cameraFocus;
    [SerializeField] private PhotoScore photoScore;
    [SerializeField] private CameraFlash cameraFlash; 
    [SerializeField] private RectTransform topShutter;    
    [SerializeField] private RectTransform bottomShutter;
    [SerializeField] private TextMeshProUGUI filmCounterText; 


    [Header("Photo Display Settings")]
    [SerializeField] private GameObject photoReviewUI; 
    [SerializeField] private RawImage capturedPhotoDisplay; 
    [SerializeField] private Image whiteFlashOverlay; 
    [Range(0, 5)]
    // [SerializeField] private float waitTimeToCapture = 1.0f; 
    [SerializeField] private float shutterSpeed = 0.15f; 
    [SerializeField] private float photoReviewTime = 2.0f; // might tweak this so user can close out of it early


    [Header("Settings")]
    [SerializeField] private int maxFilm = 10; 
    [SerializeField] private int currFilm;
    

    // private CharacterController controller; This was never used 
    enum CaptureState
    {
        Idle,
        Capturing
    };

    private CaptureState currentState;
    private bool isReview = false; 
    private float shutterOpenHeight; 

    void Awake ()
    {
        // controller = GetComponent<CharacterController>(); 
        if (inputManager == null) inputManager = GetComponent<InputManager>(); 
        if (cameraFlash == null) cameraFlash = GetComponentInChildren<CameraFlash>();

    }

    void Start()
    {
        // Initialize Film 
        currFilm = maxFilm; 
        currentState = CaptureState.Idle;

        if (topShutter != null)
        {
            shutterOpenHeight = topShutter.rect.height; 
            SetShuttersOpen(); 
        }

        if (mainCam != null) mainCam.SetActive(true); 
        if (photoCam != null) photoCam.SetActive(false); 
        if (viewFinderUI != null) viewFinderUI.SetActive(false); 
        if (photoReviewUI != null) photoReviewUI.SetActive(false); 

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnInteract += Interact;        
            inputManager.OnShoot += Shoot;   
        }
    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if (isReview) return; 

        if(isCapturing)
        {
            currentState = CaptureState.Capturing;
            mainCam.SetActive(false);
            photoCam.SetActive(true);
            viewFinderUI.SetActive(true);
            if (filmCounterText != null) filmCounterText.text = $"{currFilm} Shots";
            Debug.Log("CameraRaised"); 
        } 
        
        else
        {
            currentState = CaptureState.Idle;
            mainCam.SetActive(true);
            photoCam.SetActive(false);
            viewFinderUI.SetActive(false); 
            Debug.Log("CameraLowered"); 
        }
        
    }

    private void Interact()
    {
        // I made left click to shoot but if we want to keep it I kept the logic 
        // switch (currentState)
        // {
        //     case CaptureState.Idle: Debug.Log("Interacting"); break;
        //     case CaptureState.Capturing: AttemptTakePhoto(); break;
        // }

        if (currentState == CaptureState.Idle) Debug.Log("Interacting"); 

    }

    private void Shoot()
    {
        if (currentState == CaptureState.Capturing && !isReview) AttemptTakePhoto(); 
    }

    void OnDestroy()
    {
        if (inputManager != null) 
        {
            inputManager.OnAim -= UpdateCaptureState;
            inputManager.OnInteract -= Interact;
            inputManager.OnShoot -= Shoot; 
        }
    }

    // TODO: actually implement taking the photo
    // setup start for basic scoring with focusing 
    // private void TakePhoto()
    // {
    //     currFilm --; 
    //     // if (cameraFlash != null) cameraFlash.TriggerFlash(); 
    //     // float score = 0f;
    //     // if (cameraFocus != null) score = cameraFocus.GetFocusScore();
    //     // Debug.Log($"Photo taken, Focus Quality: {score * 100:F0}%");
    //     StartCoroutine(CapturePhotoRoutine());
    // }

    // TODO: add some sort of ui feedback to indicate that the user is out of film 
    private void AttemptTakePhoto()
    {
        if (currFilm > 0)
        {
            currFilm --; 
            StartCoroutine(CapturePhotoRoutine()); 
        }
        
        else Debug.Log("Camera out of film"); 
    }

    // TODO: after prototype need to implement a way to exit out of preview early 
    // routine that captures the photo and displays it 
    private IEnumerator CapturePhotoRoutine()
    {
        isReview = true; 
        
        if (photoReviewUI != null) photoReviewUI.SetActive(true); 
        if (capturedPhotoDisplay != null) capturedPhotoDisplay.gameObject.SetActive(false); 

        yield return StartCoroutine(AnimateShutters(shutterOpenHeight, 0f, shutterSpeed)); 

        if (cameraFlash != null) cameraFlash.TriggerFlash(); 
        if (viewFinderUI != null) viewFinderUI.SetActive(false); 

        yield return new WaitForEndOfFrame(); 

        Texture2D screenCap = ScreenCapture.CaptureScreenshotAsTexture();
        if (capturedPhotoDisplay != null)
        {
            capturedPhotoDisplay.texture = screenCap;
            capturedPhotoDisplay.gameObject.SetActive(true); 
        }

        yield return StartCoroutine(AnimateShutters(0f, shutterOpenHeight, shutterSpeed)); 

        if (cameraFocus != null)
        {
            float score = cameraFocus.GetFocusScore();
            Debug.Log($"Photo taken, Focus Quality: {score * 100:F0}%");
        }

        yield return new WaitForSeconds(photoReviewTime); 

        if (photoReviewUI != null) photoReviewUI.SetActive(false); 
        if (viewFinderUI != null && currentState == CaptureState.Capturing) viewFinderUI.SetActive(true); 

        isReview = false; 
    }

    private IEnumerator AnimateShutters(float startY, float endY, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float curve = Mathf.Sin(percent * Mathf.PI * 0.5f); 
            float currentY = Mathf.Lerp(startY, endY, curve);

            if (topShutter != null) topShutter.anchoredPosition = new Vector2(0, currentY);
            if (bottomShutter != null) bottomShutter.anchoredPosition = new Vector2(0, -currentY);

            yield return null;
        }

        if (topShutter != null) topShutter.anchoredPosition = new Vector2(0, endY);
        if (bottomShutter != null) bottomShutter.anchoredPosition = new Vector2(0, -endY);
    }

    private void SetShuttersOpen()
    {
        if (topShutter != null) topShutter.anchoredPosition = new Vector2(0, shutterOpenHeight);
        if (bottomShutter != null) bottomShutter.anchoredPosition = new Vector2(0, -shutterOpenHeight);
    }

}