using UnityEngine;

/*
    This script keeps track of the camera and scoring states
*/

public class CameraController : MonoBehaviour
{
    //Camera Type
    public CameraScriptable currentCamera;

    public enum CaptureState
    {
        Idle,
        Capturing,
        Developing,
        Reviewing,
    };

    [Header("Settings")]
    [SerializeField] private int currFilm;
    [SerializeField] private int maxFilm = 10;

    private CaptureState currentState;
    private bool hasPendingPhoto;

    //Other Scripts
    private InputManager inputManager;
    private CaptureSystem captureSystem;
    private Development development;
    private PhotoScoring eval;
    private PlayerUIManager ui;

    //Camera Features
    private CameraZoom cameraZoom;
    private CameraAutoFocus autoFocus;
    private CameraManualFocus manualFocus;

    private bool cameraRaised;

    private ScoreParameters currentResult;

    void Awake()
    {
        inputManager = GetComponentInParent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnShoot += HandleCaptureAction;
        }

        captureSystem = GetComponent<CaptureSystem>();
        development = GetComponent<Development>();
        eval = GetComponent<PhotoScoring>();

        ui = GetComponentInParent<Transform>().parent.GetComponentInChildren<PlayerUIManager>();

        cameraZoom = GetComponentInChildren<CameraZoom>();
        autoFocus = GetComponentInChildren<CameraAutoFocus>();
        manualFocus = GetComponentInChildren<CameraManualFocus>();


    }

    void Start()
    {
        TransitionToState(CaptureState.Idle);
        currFilm = maxFilm;
    }

    void Update()
    {
        //Check if done Developing
        if(currentState == CaptureState.Developing && development.IsDevelopComplete())
        {
            EndDevelopment(development.GetDevelopPercent());
        }
    }

    private void UpdateCaptureState(bool isActive)
    {
        if (currentState == CaptureState.Reviewing) return;

        if (isActive)
        {
            if (hasPendingPhoto)
            {
                TransitionToState(CaptureState.Developing);
            } else
            {
                TransitionToState(CaptureState.Capturing);
            }

        } else if (!isActive)
        {
            if (currentState == CaptureState.Capturing || currentState == CaptureState.Developing)
            {
                TransitionToState(CaptureState.Idle);
            }
        }
    }

    private void TransitionToState(CaptureState nextState)
    {
        currentState = nextState;
        Debug.Log(currentState);

        cameraRaised = currentState == CaptureState.Capturing;

        if (currentState == CaptureState.Developing)development.ToggleDevelopment(true);
        else development.ToggleDevelopment(false);

        //Cursor Handling
        if (currentState == CaptureState.Reviewing)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ApplyFeatureState(currentState);
        ui.UpdateCanvasState(cameraRaised);
        //Method from Player interaction to update state
    }

    private void HandleCaptureAction()
    {
        
        if (currentState == CaptureState.Capturing) //Attempts to Capture Photo
        {
            if (currFilm <= 0)
            {
                Debug.Log("No more Film");
                return; 
            }

            if (captureSystem.CaptureSubject())
            {
                hasPendingPhoto = true;
                currFilm -= 1;
                Debug.Log(currFilm + " Shoots Left");
                TransitionToState(CaptureState.Idle);
            }
        } else if (currentState == CaptureState.Developing) //Ends Development prematurely
        {
            EndDevelopment(development.GetDevelopPercent());
        }
    }


    private void ApplyFeatureState(CaptureState state)
    {
        bool capturing = (state == CaptureState.Capturing);

        cameraZoom.SetActive(capturing);
        autoFocus.SetActive(capturing && !currentCamera.manualFocus);
        manualFocus.SetActive(capturing && currentCamera.manualFocus);
    }

    private void EndDevelopment(float developPercent)
    {
        development.ResetDevelopment();
        hasPendingPhoto = false;
        eval.EvaluatePostData(developPercent);
        
        StartReview();
    }

    private void StartReview()
    {
        TransitionToState(CaptureState.Reviewing);

        currentResult = eval.CalculatePhotoScore();

        Debug.Log("Toal: " + currentResult.result);
        Debug.Log("dist: " + currentResult.distance);
        Debug.Log("facing: " + currentResult.facing);
        Debug.Log("size: " + currentResult.size);
        Debug.Log("focus: " + currentResult.focus);
        Debug.Log("devlop: " + currentResult.development);
        Debug.Log("extras: " + currentResult.extras);

        ui.DisplayResults(currentResult);

        Time.timeScale = 0f; 
    }

    public void EndReview()
    {
        Debug.Log("wow");
        TransitionToState(CaptureState.Idle);
        Destroy(currentResult.currentPhoto);

        Time.timeScale = 1f; 
    }

    public bool HasPendingPhoto()
    {
        return hasPendingPhoto;
    }

    public bool getCameraState()
    {
        return cameraRaised;
    }


    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnAim -= UpdateCaptureState;
            inputManager.OnShoot -= HandleCaptureAction;
        }
    }
}
