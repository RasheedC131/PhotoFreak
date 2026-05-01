using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitToDesktopButton;

    [Header("Settings Panel")]
    [SerializeField] private Button settingsBackButton;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    [Header("Optional References")]
    [SerializeField] private InputManager inputManager;

    void Awake()
    {
        // Default to hidden – pause panel only appears when GlobalGameState pauses.
        // Toggling the wrapper hides the dim backdrop too; falling back to toggling
        // panels individually keeps the old behaviour for setups without a menuRoot.
        if (menuRoot != null) menuRoot.SetActive(false);
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
        if (menuRoot != null)      menuRoot.SetActive(true);
        if (pausePanel != null)    pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void HidePause()
    {
        if (pausePanel != null)    pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        // Toggle the wrapper *last* so any nested layout updates settle before
        // it's deactivated — also hides the dim backdrop in one shot.
        if (menuRoot != null)      menuRoot.SetActive(false);
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
        // Restore time scale before changing scenes – paused timeScale stays at 0 otherwise and the menu scene freezes.
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
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
