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

    void Awake()
    {
        if (inputManager == null) inputManager = GetComponentInParent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnShoot += AttemptCapture;
        }
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
                Debug.Log("Developing");
            } else
            {
                TransitionToState(CaptureState.Capturing);
                Debug.Log("Capturing");
            }

        } else if (!isActive)
        {
            if (currentState == CaptureState.Capturing || currentState == CaptureState.Developing)
            {
                TransitionToState(CaptureState.Idle);
                Debug.Log("Stopped");
            }
        }
    }

    private void TransitionToState(CaptureState nextState)
    {
        currentState = nextState;

        //Method from UI to update state
        //Method from Player interaction to update state
    }

    private void AttemptCapture()
    {
        if (currentState == CaptureState.Capturing)
        {
            //Method from another script to handle actual capture and scoring
            hasPendingPhoto = true;
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
