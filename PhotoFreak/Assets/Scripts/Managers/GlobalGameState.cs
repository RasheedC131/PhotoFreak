using UnityEngine;
using System; 

public class GlobalGameState : MonoBehaviour
{
    public enum GameState { PLAYING, PAUSED, GAMEOVER }
    public GameState currentState { get; private set; }
    
    public static GlobalGameState instance; 

    public event Action onGamePaused; 
    public event Action onGameResumed; 
    public event Action onGameOver; 

    [SerializeField] private InputManager inputManager; 

    void Awake()
    {
        if (instance == null) instance = this; 
        else Destroy(gameObject); 
    }

    void Start()
    {
        currentState = GameState.PLAYING; 

        if (inputManager != null)
        {
            inputManager.OnPause += GamePaused; 
            inputManager.OnResume += GameResumed; 
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
        currentState = GameState.GAMEOVER; 
        Time.timeScale = 1f; 
        onGameOver?.Invoke();
        Debug.Log("Game Over"); 
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
