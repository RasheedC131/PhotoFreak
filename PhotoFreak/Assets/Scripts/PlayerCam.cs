using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private InputManager inputManager; 
    [SerializeField] private Transform orientation; 

    public float sensX = 20f;
    public float sensY = 20f;

    [Range(-90f, 0f)]
    [SerializeField] private float topClamp = -90f; 

    [Range(0f, 90f)]
    [SerializeField] private float bottomClamp = 90f; 



    private float xRot; 
    private float yRot; 

    Vector2 mouseInput; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inputManager != null) inputManager.OnLook += HandleLookInput; 
    }

    void OnDestroy()
    {
        if (inputManager != null) inputManager.OnLook -= HandleLookInput; 
    }

    private void HandleLookInput (Vector2 input)
    {
        mouseInput = input; 
    }

    // Update is called once per frame
    void Update()
    {
        //Get Input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRot += mouseX;
        xRot -= mouseY;

        xRot = Mathf.Clamp(xRot, topClamp, bottomClamp); // Restricts Vertical Rotation

        transform.localRotation = Quaternion.Euler(xRot, 0, 0); // Rotates Camera

        if (orientation != null)  orientation.Rotate(Vector3.up * mouseX); // Rotate Player orientation
    }
}
