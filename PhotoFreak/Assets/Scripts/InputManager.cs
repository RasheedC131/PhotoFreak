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

    // TODO: still needs to be implemented 
    public event Action OnPause; 
    public event Action OnResume; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerControls = new PlayerControls(); 
    }

    private void OnEnable()
    {
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

        // playerControls.Ground.Pause.performed += ctx => TogglePause(); 
        // playerControls.UI.Resume.performed += ctx => TogglePause();  
        
        EnableGameplayControls(); 
    }

    private void OnDisable()
    {
        playerControls.Ground.Disable(); 
    }

    public void EnableGameplayControls()
    {
        if (playerControls.Ground.enabled)
        {
            OnPause?.Invoke(); 
            // EnableUIControls(); 
        }

        else
        {
            OnResume?.Invoke(); 
            // EnableGameplayControls(); 
        }
    }
}
