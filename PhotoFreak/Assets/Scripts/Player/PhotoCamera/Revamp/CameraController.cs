using UnityEngine;

/*
    This script keeps track of the camera and scoring states
*/

public class CameraController : MonoBehaviour
{
    //Camera Type
    [SerializeField] private CameraScriptable currentCamera;


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

    void Awake()
    {
        if (inputManager == null) inputManager = GetComponentInParent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnShoot += AttemptCapture;
        }

        captureSystem = GetComponent<CaptureSystem>();
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
        }
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
