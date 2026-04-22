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

    private CaptureState currentState;
    private bool hasPendingPhoto;

    //Other Scripts
    private InputManager inputManager;
    private CaptureSystem captureSystem;

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
            inputManager.OnShoot += AttemptCapture;
        }

        captureSystem = GetComponent<CaptureSystem>();

        cameraZoom = GetComponentInChildren<CameraZoom>();
        autoFocus = GetComponentInChildren<CameraAutoFocus>();
        manualFocus = GetComponentInChildren<CameraManualFocus>();


    }

    void Start()
    {
        currentState = CaptureState.Idle;
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

        ApplyFeatureState(currentState);
        //Method from UI to update state
        //Method from Player interaction to update state
    }

    private void AttemptCapture()
    {
        if (currentState == CaptureState.Capturing) //Returns bool based on success of capture
        {
            if (captureSystem.CaptureSubject())
            {
                hasPendingPhoto = true;
                TransitionToState(CaptureState.Idle);
            }
        } else if (currentState == CaptureState.Developing) //Destroys photo (for debugging)
        {
            hasPendingPhoto = false;
            TransitionToState(CaptureState.Idle);
        }
    }

    private void ApplyFeatureState(CaptureState state)
    {
        bool capturing = (state == CaptureState.Capturing);

        cameraZoom.SetActive(capturing);
        autoFocus.SetActive(capturing && !currentCamera.manualFocus);
        manualFocus.SetActive(capturing && currentCamera.manualFocus);
    }


    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnAim -= UpdateCaptureState;
            inputManager.OnShoot -= AttemptCapture;
        }
    }
}
