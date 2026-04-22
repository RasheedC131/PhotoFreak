using UnityEngine;

public class CameraAutoFocus : MonoBehaviour
{
    private bool isActive = false;

    private float focus = 0f; // 0-1
    private float maxFocus = 1f;
    private float minFocus = 0f;
    
    [Header("Settings")]
    [SerializeField] private float moveCost = 0.3f;
    [SerializeField] private float stabilizeRate = 0.7f;

    private bool isMoving = false;

    //Other Scripts
    private InputManager inputManager;

    void Awake()
    {
        inputManager = transform.root.GetComponent<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnMove += OnMoveInput;
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (isMoving)
        {
            focus -= moveCost * Time.deltaTime;
        } else
        {
            focus += stabilizeRate * Time.deltaTime;
        }
        
        focus = Mathf.Clamp(focus, minFocus, maxFocus);
        //Debug.Log(focus);
    }

    private void OnMoveInput(Vector2 input)
    {
        if (!isActive) return;

        isMoving = input.sqrMagnitude > 0.01f;
    }

    private void ResetFocus()
    {
        focus = minFocus;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (!isActive)
        {
            ResetFocus();
        }
    }

    public float GetFocus()
    {
        return focus;
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnMove -= OnMoveInput;
        }
    }
}
