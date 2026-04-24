using UnityEngine;

/*
    This script keeps track of the camera and scoring states
*/

public class CameraController : MonoBehaviour
{
    //Camera Type
    public CameraScriptable currentCamera;

    //States
    enum CaptureState
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

    //Camera Features
    private CameraZoom cameraZoom;
    private CameraAutoFocus autoFocus;
    private CameraManualFocus manualFocus;

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

        cameraZoom = GetComponentInChildren<CameraZoom>();
        autoFocus = GetComponentInChildren<CameraAutoFocus>();
        manualFocus = GetComponentInChildren<CameraManualFocus>();


    }

    void Start()
    {
        currentState = CaptureState.Idle;
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

        if (currentState == CaptureState.Developing)development.ToggleDevelopment(true);
        else development.ToggleDevelopment(false);

        ApplyFeatureState(currentState);


        //Method from UI to update state
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
        eval.ScoreDevelopment(developPercent);
        TransitionToState(CaptureState.Idle);
    }

    public bool HasPendingPhoto()
    {
        return hasPendingPhoto;
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
