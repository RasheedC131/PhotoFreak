using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI; 
using System.Collections; 
using TMPro; 


public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private GameObject viewFinderUI; 
    [SerializeField] private RectTransform topShutter;    
    [SerializeField] private RectTransform bottomShutter;
    [SerializeField] private TextMeshProUGUI filmCounterText; 
    [SerializeField] private MonoBehaviour cameraLookScript; 
    [SerializeField] private MonoBehaviour playerMovementScript;


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

        if (viewFinderUI != null) viewFinderUI.SetActive(false); 
        if (photoReviewUI != null) photoReviewUI.SetActive(false); 

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnInteract += Interact;        
            inputManager.OnShoot += Shoot;   
        }


        //Getting Scripts
        photoScore = GetComponent<PhotoScore>();
        cameraFocus = GetComponent<CameraFocus>();
        cameraFlash = GetComponent<CameraFlash>();

    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if (isReview) return; 

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

    // TODO: add some sort of ui feedback to indicate that the user is out of film 
    private void AttemptTakePhoto()
    {
        if (currFilm > 0)
        {
            currFilm --; 
            if (filmCounterText != null) filmCounterText.text = $"{currFilm} Shots";
            photoScore.CaptureSubject();
            StartCoroutine(CapturePhotoRoutine()); 
        }
        
        else Debug.Log("Camera out of film"); 
    }

    // TODO: after prototype need to implement a way to exit out of preview early 

    // routine that captures the photo and displays it 
    private IEnumerator CapturePhotoRoutine()
    {
        isReview = true; 

        // disable player from being able to look/move
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;


        ResetStars(); 

        if (photoReviewUI != null) photoReviewUI.SetActive(true);         
        if (capturedPhotoDisplay != null) capturedPhotoDisplay.gameObject.SetActive(false); 

        // close shutter
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

        // calculate score and display stars

        // open shutter
        yield return StartCoroutine(AnimateShutters(0f, shutterOpenHeight, shutterSpeed)); 
    
        yield return new WaitForSeconds(0.2f); 
        CalculateAndShowStars(); 

        if (cameraFocus != null)
        {
            float score = cameraFocus.GetFocusScore();
            Debug.Log($"Photo taken, Focus Quality: {score * 100:F0}%");
        }

        yield return new WaitForSeconds(photoReviewTime); 

        // clean up the states 
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        if (photoReviewUI != null) photoReviewUI.SetActive(false); 
        if (viewFinderUI != null && currentState == CaptureState.Capturing) viewFinderUI.SetActive(true); 

        isReview = false;
        UpdateCaptureState(false);
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
        if (cameraFocus == null) return;

        float score = cameraFocus.GetFocusScore();
        int starCount = Mathf.RoundToInt(score * 5);

        Debug.Log($"Score: {score:F2}, Stars: {starCount}");
        UpdateStarUI(starCount);
    }

}

