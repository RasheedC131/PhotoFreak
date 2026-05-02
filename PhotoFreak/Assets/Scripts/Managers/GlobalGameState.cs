using UnityEngine;
using System; 
using UnityEngine.SceneManagement;

public class GlobalGameState : MonoBehaviour
{
    public enum GameState { PLAYING, PAUSED, GAMEOVER }
    public GameState currentState { get; private set; }
    
    public static GlobalGameState Instance; 

    public event Action onGamePaused; 
    public event Action onGameResumed; 
    public event Action onGameOver; 

    [SerializeField] private InputManager inputManager; 
    
    // Add a tracker for when the scene started
    private float sceneStartTime; 

    void Awake()
    {
        if (Instance == null) Instance = this; 
        else Destroy(gameObject); 
    }

    void Start()
    {
        currentState = GameState.PLAYING;

        // Use realtime so it ignores Time.timeScale freezing
        sceneStartTime = Time.realtimeSinceStartup;

        if (inputManager == null) inputManager = FindFirstObjectByType<InputManager>();

        if (inputManager != null)
        {
            inputManager.OnPause += GamePaused;
            inputManager.OnResume += GameResumed;
        }
        else
        {
            Debug.LogWarning("[GlobalGameState] No InputManager found — pause/resume events won't fire.");
        }
    }

    public void GamePaused()
    {
        if (currentState != GameState.PLAYING) return; 

        currentState = GameState.PAUSED; 
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 
        onGamePaused?.Invoke(); 
        Debug.Log("Game Paused"); 
    }

    public void GameResumed()
    {
        if (currentState != GameState.PAUSED) return; 

        currentState = GameState.PLAYING; 
        Time.timeScale = 1f; 
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
        onGameResumed?.Invoke(); 
        Debug.Log("Game Resumed"); 
    }

    public void TriggerGameOver()
    {
        // wait one sec for game over 
        if (Time.realtimeSinceStartup - sceneStartTime < 1.0f) return;

        currentState = GameState.GAMEOVER; 
        Time.timeScale = 0f; 
        onGameOver?.Invoke();
        Debug.Log("Game Over"); 
        SceneManager.LoadScene("TitleScreen");
    }

    void OnDestroy ()
    {
        if (inputManager != null)
        {
            inputManager.OnPause -= GamePaused; 
            inputManager.OnResume -= GameResumed; 
        }
    }
}