using UnityEngine;
using UnityEngine.InputSystem; 
using System;

public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private GameObject viewFinderUI;

    [Header("Settings")]
    [SerializeField] private int maxFilm = 10; 
    [SerializeField] private int currFilm;

    private PhotoScore photoScore;

    
    enum CaptureState
    {
        Idle,
        Capturing
    };

    private CaptureState currentState;

    void Awake ()
    {
        if (inputManager == null) inputManager = GetComponent<InputManager>(); 

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize Film 
        currFilm = maxFilm; 
        currentState = CaptureState.Idle;

        if (viewFinderUI != null) viewFinderUI.SetActive(false); 

        if (inputManager != null)
        {
            inputManager.OnAim += UpdateCaptureState;
            inputManager.OnInteract += Interact;        
            inputManager.OnShoot += Shoot;   
        }

        photoScore = GetComponent<PhotoScore>();
    }

    private void UpdateCaptureState(bool isCapturing)
    {
        if(isCapturing)
        {
            currentState = CaptureState.Capturing;

            viewFinderUI.SetActive(true); 
            Debug.Log("CameraRaised"); 
        } 
        
        else
        {
            currentState = CaptureState.Idle;
    
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
        photoScore.CaptureSubject();
    }

    // TODO: add some sort of ui feedback to indicate that the user is out of film 
    private void AttemptTakePhoto()
    {
        if (currFilm > 0) TakePhoto();
        else Debug.Log("Camera out of film"); 
    }

}



