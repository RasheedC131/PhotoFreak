using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainLevel";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // TODO: 
    public void QuitGame()
    {
        Debug.Log("Quit Pressed");
        Application.Quit();
    }
}