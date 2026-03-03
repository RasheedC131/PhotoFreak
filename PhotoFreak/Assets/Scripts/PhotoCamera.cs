using UnityEngine;
using UnityEngine.InputSystem; 
using System;

public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject photoCam;

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

    void Awake ()
    {
        // controller = GetComponent<CharacterController>(); 
        if (inputManager == null) inputManager = GetComponent<InputManager>(); 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize Film 
        currFilm = maxFilm; 
        currentState = CaptureState.Idle;

        if (mainCam != null) mainCam.SetActive(true); 
        if (photoCam != null) photoCam.SetActive(false); 

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnInteract += Interact;        
            inputManager.OnShoot += Shoot;   
        }
    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if(isCapturing)
        {
            currentState = CaptureState.Capturing;
            mainCam.SetActive(false);
            photoCam.SetActive(true);
            Debug.Log("CameraRaised"); 
        } 
        
        else
        {
            currentState = CaptureState.Idle;
            mainCam.SetActive(true);
            photoCam.SetActive(false);
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
        if (currentState == CaptureState.Capturing) AttemptTakePhoto(); 
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
    private void TakePhoto()
    {
        currFilm --; 
        Debug.Log($"Took Photo, Film remaining: {currFilm}"); 
    }

    // TODO: add some sort of ui feedback to indicate that the user is out of film 
    private void AttemptTakePhoto()
    {
        if (currFilm > 0) TakePhoto();
        else Debug.Log("Camera out of film"); 
    }

}



