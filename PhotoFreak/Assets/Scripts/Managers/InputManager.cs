using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls; 
    private bool isPaused = false; 

    // events 
    public event Action<Vector2> OnMove; 
    public event Action<Vector2> OnLook; 
    public event Action<bool> OnSprint; 
    public event Action <bool> OnCrouch;
    public event Action<bool> OnAim;
    public event Action OnInteract;
    public event Action OnShoot; 
    public event Action<float> OnZoom; 
    public event Action<float> OnFocus; 
    public event Action OnPause; 
    public event Action OnResume; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        
    }

    void Awake()
    {
        playerControls = new PlayerControls(); 

        playerControls.Ground.Enable();

        playerControls.Ground.Movement.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());        // wasd
        playerControls.Ground.Movement.canceled += ctx => OnMove?.Invoke(Vector2.zero); 

        playerControls.Ground.Look.performed += ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>());        
        playerControls.Ground.Look.canceled += ctx => OnLook?.Invoke(Vector2.zero); 


        playerControls.Ground.Sprint.performed += ctx => OnSprint?.Invoke(true); 
        playerControls.Ground.Sprint.canceled += ctx => OnSprint?.Invoke(false); 

        playerControls.Ground.Crouch.performed += ctx => OnCrouch?.Invoke(true); 
        playerControls.Ground.Crouch.canceled += ctx => OnCrouch?.Invoke(false); 

        playerControls.Ground.Shoot.performed += ctx => OnShoot?.Invoke(); 

        playerControls.Ground.Pause.performed += ctx => TogglePause(); 
        playerControls.UIMap.Resume.performed += ctx => TogglePause();  

        playerControls.Ground.Aim.started += ctx => OnAim?.Invoke(true); 
        playerControls.Ground.Aim.canceled += ctx => OnAim?.Invoke(false);

        playerControls.Ground.Interact.performed += ctx => OnInteract?.Invoke(); 

        playerControls.Ground.ScrollAction.performed += ctx =>
        {
            float scrollVal = ctx.ReadValue<Vector2>().y; 
            
            // deadzone 
            if (Mathf.Abs(scrollVal) < 0.01f) return;

            // Focus
            if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed) OnFocus?.Invoke(scrollVal);
            
            // zoom 
            else OnZoom?.Invoke(scrollVal);
        };
    }

    private void OnEnable()
    {
        // InitializeControls();
        playerControls.Ground.Enable();         
    }

    private void OnDisable()
    {
        playerControls.Ground.Disable(); 
    }

    public void TogglePause()
    {

        isPaused = !isPaused; 

        if (isPaused)
        {
            playerControls.Ground.Disable(); 
            playerControls.UIMap.Enable(); 
            OnPause?.Invoke(); 
        }

        else
        {
            playerControls.Ground.Enable(); 
            playerControls.UIMap.Disable(); 
            OnResume?.Invoke(); 
        }
    }
}
