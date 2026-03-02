using UnityEngine;
using UnityEngine.InputSystem; 
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager; 
    private CharacterController controller; 

    [Header("Movement and Speed")]
    public float walkSpeed = 6f; 
    public float sprintSpeed = 9f; 
    public float crouchSpeed = 3f; 

    [Header("Jump and Gravity")]
    public float jumpHeight = 2f; 
    public float gravity = -9.81f; 

    [Header("Crouching")]
    public float crouchHeight = 1f; 
    public float standingHeight = 2f; 
    public float crouchTransitionSpeed = 10f; 

    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0); 
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);


    // tracks our current state
    private Vector2 currMovementInput; 
    private float currSpeed = 0f; 
    private Vector3 velocity; 
    private bool isGrounded; 
    private bool isSprinting; 
    private bool isCrouching; 

    void Awake ()
    {
        controller = GetComponent<CharacterController>(); 
        if (inputManager == null) inputManager = GetComponent<InputManager>(); 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currSpeed = walkSpeed; 

        // subscribe to our move events 
        if (inputManager != null)
        {
            inputManager.OnMove += UpdateMoveInput; 
            inputManager.OnJump += Jump; 
            inputManager.OnSprint += ToggleSprint; 
            inputManager.OnCrouch += ToggleCrouch; 
        }
    }

    void Update()
    {
        ApplyGravity(); 
        HandleStance(); 
        MovePlayer(); 
    }

    private void ApplyGravity()
    {
        isGrounded = controller.isGrounded; 

        if (isGrounded && velocity.y < 0) velocity.y = -2f; 

        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleStance()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight; 
        Vector3 targetCenter = isCrouching ? crouchCenter : standingCenter; 
    
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed); 
    }

    private void MovePlayer()
    {
        if (isCrouching) currSpeed = crouchSpeed; 
        else if (isSprinting) currSpeed = sprintSpeed; 
        else currSpeed = walkSpeed; 

        // horizontal movement 
        Vector3 moveDirection = new Vector3 (currMovementInput.x, 0, currMovementInput.y); 
        Vector3 finalMove = transform.TransformDirection(moveDirection) * currSpeed; 

        // jump and gravity movement 
        finalMove.y = velocity.y; 

        controller.Move(finalMove * Time.deltaTime); 

    }

    void OnDestroy()
    {
        // unsubscribes from the object to prevent leaks 
        if (inputManager != null)
        {
            inputManager.OnMove -= UpdateMoveInput; 
            inputManager.OnJump -= Jump; 
            inputManager.OnSprint -= ToggleSprint; 
            inputManager.OnCrouch -= ToggleCrouch; 
        }
    }

    // event listeners 
    private void UpdateMoveInput(Vector2 input)
    {
        currMovementInput = input; 
    }

    private void Jump()
    {
        if (isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); 
    }

    private void ToggleSprint(bool isSprinting)
    {
        this.isSprinting = isSprinting; 
    }

    private void ToggleCrouch(bool isCrouching)
    {
        this.isCrouching = isCrouching; 
    }

}
