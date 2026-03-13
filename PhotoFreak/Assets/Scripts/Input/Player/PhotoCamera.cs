using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI; 
using System.Collections; 
using TMPro; 

public class PhotoCamera : MonoBehaviour, IEquippable
{
    enum CaptureState
    {
        Idle,
        Capturing,
    };

    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private GameObject viewFinderUI; 
    [SerializeField] private RectTransform topShutter;    
    [SerializeField] private RectTransform bottomShutter;
    [SerializeField] private TextMeshProUGUI filmCounterText; 
    [SerializeField] private MonoBehaviour cameraLookScript; 
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Game Loop Settings")]
    [SerializeField] private FreakMeter freakMeter;
    [SerializeField] private string guestTag = "Guest"; 
    [SerializeField] private string monsterTag = "Monster"; 
    [SerializeField] private float freakPenaltyAmount = 25.0f; 
    [SerializeField] private int monsterPoints = 1000; 

    [Header("Photo Display Settings")]
    [SerializeField] private GameObject photoReviewUI; 
    [SerializeField] private RawImage capturedPhotoDisplay; 
    [SerializeField] private float shutterSpeed = 0.15f; 
    [SerializeField] private float photoReviewTime = 2.0f; // might tweak this so user can close out of it early

    [Header("Film Settings")]
    [SerializeField] private int maxFilm = 10; 
    [SerializeField] private int currFilm;

    //Scripts
    private PhotoScore photoScore;
    private CameraFocus cameraFocus;
    private CameraFlash cameraFlash;


    [Header("Star Settings")]
    [SerializeField] private Image[] starImages; 
    [SerializeField] private Color earnedStarColor = Color.yellow; 
    [SerializeField] private Color emptyStarColor = Color.gray;

    private int totalScore = 0; 
    
    private bool cameraRaised; // flag for checking if camera is raised for freakmeter
    // private CharacterController controller; This was never used 

    private CaptureState currentState;
    private bool isReview = false; 
    private float shutterOpenHeight; 

    void Awake ()
    {
        if (inputManager == null) inputManager = GetComponentInParent<InputManager>(); 
        
        photoScore = GetComponent<PhotoScore>();
        cameraFocus = GetComponent<CameraFocus>();
        cameraFlash = GetComponentInChildren<CameraFlash>(); 

        if (topShutter != null)
        {
            shutterOpenHeight = topShutter.rect.height; 
            SetShuttersOpen(); 
        }

    }

    void Start()
    {
        // Initialize Film 
        currFilm = maxFilm;
        currentState = CaptureState.Idle;

        if (viewFinderUI != null) viewFinderUI.SetActive(false); 
        if (photoReviewUI != null) photoReviewUI.SetActive(false); 

        // Safely check if cameraFocus exists before calling methods on it
        if (cameraFocus != null) 
        {
            cameraFocus.DisableDepthOfField();
        }

    }

    public void OnEquip()
    {
        gameObject.SetActive(true);
        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            // inputManager.OnInteract += Interact;
        }
    }

    public void OnUnequip()
    {
        if (inputManager != null)
        {
            inputManager.OnAim -= UpdateCaptureState;
            // inputManager.OnInteract -= Interact;
        }

        UpdateCaptureState(false);
        if (photoReviewUI != null) photoReviewUI.SetActive(false);
        isReview = false;
        
        gameObject.SetActive(false);
    }

    public void OnUse()
    {
        if (currentState == CaptureState.Capturing && !isReview) AttemptTakePhoto(); 
    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if (isReview) return; 

        cameraRaised = isCapturing;
        if(isCapturing)
        {
            currentState = CaptureState.Capturing;
            viewFinderUI.SetActive(true);
            if (filmCounterText != null) filmCounterText.text = $"{currFilm} Shots";
            Debug.Log("CameraRaised");

            cameraFocus.EnableDepthOfField();
        } 
        
        else
        {
            currentState = CaptureState.Idle;
            viewFinderUI.SetActive(false); 
            Debug.Log("CameraLowered");

           cameraFocus.DisableDepthOfField();
        }
        
    }

    void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnInteract += Interact;
            inputManager.OnShoot += Shoot; 

        }
    }

    void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnAim -= UpdateCaptureState;
            inputManager.OnInteract -= Interact;
            inputManager.OnShoot -= Shoot; 
        }
        
        // Safety cleanup when disabled
        UpdateCaptureState(false);
        if (photoReviewUI != null) photoReviewUI.SetActive(false);
        isReview = false;
    }

    private void Shoot()
    {
        if (currentState == CaptureState.Capturing && !isReview) 
        {
            AttemptTakePhoto(); 
        }
    }

    // TODO: implement once we have our inventory system to switch from camera, journal and item
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

    // TODO: add some sort of ui feedback to indicate that the user is out of film 
    private void AttemptTakePhoto()
    {
        if (currFilm > 0)
        {
            currFilm --; 
            if (filmCounterText != null) filmCounterText.text = $"{currFilm} Shots";
            // photoScore.CaptureSubject();
            StartCoroutine(CapturePhotoRoutine()); 
        }
        
        else Debug.Log("Camera out of film"); 
    }

    // TODO: after prototype need to implement a way to exit out of preview early 
    // routine that captures the photo and displays it 
    private IEnumerator CapturePhotoRoutine()
    {
        isReview = true; 
        
        yield return new WaitForEndOfFrame(); 
        Texture2D screenCap = ScreenCapture.CaptureScreenshotAsTexture();
        Time.timeScale = 0f; 

        // Identify is target is valid/guest/monster 
        GameObject hitSubject = null;
        if (photoScore != null)
        {
            hitSubject = photoScore.CaptureSubject(); 
        }

        if (hitSubject != null)
        {

            // TODO: don't make hard-coded 
            if (hitSubject.CompareTag(guestTag) || hitSubject.CompareTag("Elite"))
            {
                Debug.Log($"Photographed a {hitSubject.tag}.");
                
                if (freakMeter != null) freakMeter.AddFreakScore(freakPenaltyAmount);
            }
            
        }

        // draw ui/animate shutter
        if (capturedPhotoDisplay != null)
        {
            capturedPhotoDisplay.texture = screenCap; 
            capturedPhotoDisplay.gameObject.SetActive(true); 
        }

        if (photoReviewUI != null) photoReviewUI.SetActive(true); 
        ResetStars(); 
        yield return StartCoroutine(AnimateShutters(shutterOpenHeight, 0f, shutterSpeed)); 
        if (cameraFlash != null) cameraFlash.TriggerFlash(); 
        CalculateAndShowStars(); 

        yield return StartCoroutine(AnimateShutters(0f, shutterOpenHeight, shutterSpeed)); 
        yield return new WaitForSecondsRealtime(photoReviewTime); 

        // cleanup the states 
        Time.timeScale = 1f; 
        if (photoReviewUI != null) photoReviewUI.SetActive(false); 
        if (viewFinderUI != null && currentState == CaptureState.Capturing) viewFinderUI.SetActive(true); 
        isReview = false;
        
        if(freakMeter != null && freakMeter.IsGameOver())
        {
            Debug.Log("Game loop done.");
        }
    }

    private IEnumerator AnimateShutters(float startY, float endY, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
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

    private void UpdateStarUI(int starCount)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount) starImages[i].color = earnedStarColor; 
            else starImages[i].color = emptyStarColor; 
        }
    }

    private void ResetStars()
    {
        if (starImages == null) return; 

        foreach (Image star in starImages)
        {
            if (star != null) star.color = Color.clear; 
        }
    }
    
    // TODO: use actual scoring system 
    private void CalculateAndShowStars()
    {
        UpdateStarUI(photoScore.currentScore);
        Debug.Log("Star Count: " + photoScore.currentScore);
    }

    public bool getCameraState()
    {
        return cameraRaised;
    }

}