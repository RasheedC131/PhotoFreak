using UnityEngine;

public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 

    [Header("Settings")]
    [SerializeField] private int maxFilm = 10; 

    [SerializeField] private int currFilm; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize Film 
        currFilm = maxFilm; 
        if (inputManager != null) inputManager.OnShoot += AttemptTakePhoto;         
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (inputManager != null) inputManager.OnShoot -= AttemptTakePhoto; 
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
