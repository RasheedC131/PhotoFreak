using UnityEngine;
using UnityEngine.InputSystem; 
using System;

public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    private CharacterController controller; 

    enum CaptureState
    {
        Idle,
        Capturing
    };

    private CaptureState currentState;

    void Awake ()
    {
        controller = GetComponent<CharacterController>(); 
        if (inputManager == null) inputManager = GetComponent<InputManager>(); 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = CaptureState.Idle;

           if (inputManager != null)
            {
                inputManager.OnAim += UpdateCaptureState;
                inputManager.OnInteract += Interact;
            }
    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if(isCapturing)
        {
            currentState = CaptureState.Capturing;
            Debug.Log("CameraRaised"); 
        } else
        {
            currentState = CaptureState.Idle;
            Debug.Log("CameraLowered"); 
        }
        
    }

    private void Interact()
    {
        switch (currentState)
        {
            case CaptureState.Idle: Debug.Log("Interacting"); break;
            case CaptureState.Capturing: Debug.Log("Captured"); break;
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
            {
                inputManager.OnAim -= UpdateCaptureState;
                inputManager.OnInteract -= Interact;
            }
    }


}
