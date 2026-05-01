using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drives the pause-menu UI. Listens to GlobalGameState pause/resume events,
/// shows / hides the pause and settings panels, and wires the main buttons.
/// Drop this on the root Canvas object that holds the pause UI.
/// </summary>
public class PauseScreen : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Root panel that contains the Resume / Settings / Quit buttons.")]
    [SerializeField] private GameObject pausePanel;
    [Tooltip("Root panel that contains the Video + Controls settings.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitToDesktopButton;

    [Header("Settings Panel")]
    [SerializeField] private Button settingsBackButton;

    [Header("Scene Navigation")]
    [Tooltip("Scene loaded when 'Quit to Menu' is pressed.")]
    [SerializeField] private string mainMenuSceneName = "Main Scene";

    [Header("Optional References")]
    [Tooltip("Optional explicit InputManager. If null, the first one in the scene is used.")]
    [SerializeField] private InputManager inputManager;

    void Awake()
    {
        // Default to hidden – pause panel only appears when GlobalGameState pauses.
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Start()
    {
        if (inputManager == null) inputManager = FindFirstObjectByType<InputManager>();

        if (resumeButton != null)         resumeButton.onClick.AddListener(OnResumeClicked);
        if (settingsButton != null)       settingsButton.onClick.AddListener(OnOpenSettings);
        if (settingsBackButton != null)   settingsBackButton.onClick.AddListener(OnCloseSettings);
        if (quitToMenuButton != null)     quitToMenuButton.onClick.AddListener(OnQuitToMenu);
        if (quitToDesktopButton != null)  quitToDesktopButton.onClick.AddListener(OnQuitToDesktop);

        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.onGamePaused  += ShowPause;
            GlobalGameState.Instance.onGameResumed += HidePause;
        }
        else
        {
            Debug.LogWarning("[PauseScreen] No GlobalGameState in scene – pause UI will not toggle.");
        }
    }

    void OnDestroy()
    {
        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.onGamePaused  -= ShowPause;
            GlobalGameState.Instance.onGameResumed -= HidePause;
        }
    }

    // ---- Panel toggling --------------------------------------------------

    private void ShowPause()
    {
        if (pausePanel != null)    pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void HidePause()
    {
        if (pausePanel != null)    pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnOpenSettings()
    {
        if (pausePanel != null)    pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OnCloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null)    pausePanel.SetActive(true);
    }

    // ---- Buttons ---------------------------------------------------------

    private void OnResumeClicked()
    {
        // Route through InputManager so input maps swap back to Ground.
        if (inputManager != null)
        {
            inputManager.TogglePause();
            return;
        }

        // Fallback: at least restore time + cursor if the manager is gone.
        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.GameResumed();
        }
    }

    private void OnQuitToMenu()
    {
        // Restore time scale before changing scenes – paused timeScale stays
        // at 0 otherwise and the menu scene freezes.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void OnQuitToDesktop()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
