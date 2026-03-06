using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls; 

    // events 
    public event Action<Vector2> OnMove; 
    public event Action<Vector2> OnLook; 
    public event Action OnJump; 
    public event Action<bool> OnSprint; 
    public event Action <bool> OnCrouch;
    public event Action<bool> OnAim;
    public event Action OnInteract;
    public event Action OnShoot; 
    public event Action<float> OnZoom; 
    public event Action<float> OnFocus; 

    // TODO: still needs to be implemented 
    public event Action OnPause; 
    public event Action OnResume; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerControls = new PlayerControls(); 

        playerControls.Ground.Enable();

        playerControls.Ground.Movement.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        playerControls.Ground.Movement.canceled += ctx => OnMove?.Invoke(Vector2.zero); 

        playerControls.Ground.Look.performed += ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>()); 
        playerControls.Ground.Look.canceled += ctx => OnLook?.Invoke(Vector2.zero); 

        playerControls.Ground.Jump.performed += ctx => OnJump?.Invoke(); 

        playerControls.Ground.Sprint.performed += ctx => OnSprint?.Invoke(true); 
        playerControls.Ground.Sprint.canceled += ctx => OnSprint?.Invoke(false); 

        playerControls.Ground.Crouch.performed += ctx => OnCrouch?.Invoke(true); 
        playerControls.Ground.Crouch.canceled += ctx => OnCrouch?.Invoke(false); 

        playerControls.Ground.Shoot.performed += ctx => OnShoot?.Invoke(); 

        // playerControls.Ground.Pause.performed += ctx => TogglePause(); 
        // playerControls.UI.Resume.performed += ctx => TogglePause();  

        playerControls.Ground.Aim.started += ctx => OnAim?.Invoke(true); 
        playerControls.Ground.Aim.canceled += ctx => OnAim?.Invoke(false);

        playerControls.Ground.Interact.performed += ctx => OnInteract?.Invoke(); 

        playerControls.Ground.Focus.performed += ctx =>
        {
            float scrollValue = ctx.ReadValue<float>(); 

            if (keyboard.current.shiftkey.isPressed) OnFocus?.Invoke(scrollValue); 
            else OnZoom?.Invoke(scrollValue); 
        };
    }

    private void OnEnable()
    {
        playerControls.Ground.Enable();         
    }

    private void OnDisable()
    {
        playerControls.Ground.Disable(); 
    }

    // Todo: tweak this later when we have resume/pause
    public void EnableGameplayControls()
    {
        // if (playerControls.Ground.enabled)
        // {
        //     OnPause?.Invoke(); 
        //     // EnableUIControls(); 
        // }

        // else
        // {
        //     OnResume?.Invoke(); 
        //     // EnableGameplayControls(); 
        // }
    }
}
