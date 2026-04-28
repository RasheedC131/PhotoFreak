using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform topShutter;
    [SerializeField] private RectTransform bottomShutter;
    [SerializeField] private Image flashImage;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button retryButton; 

    [Header("Animation Settings")]
    [SerializeField] private float shutterSpeed = 0.2f;   
    [SerializeField] private float flashDuration = 0.5f;  
    [SerializeField] private float textFadeSpeed = 1.0f;  

    private Vector2 topShutterStartPos;
    private Vector2 bottomShutterStartPos;

    void Start()
    {
        if (gameOverText != null) 
        {
            gameOverText.gameObject.SetActive(false);
            gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0f);
        }
        
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
            flashImage.color = new Color(1f, 1f, 1f, 0f);
        }

        // Hide the button at the start of the game
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }

        if (topShutter != null) topShutterStartPos = topShutter.anchoredPosition;
        if (bottomShutter != null) bottomShutterStartPos = bottomShutter.anchoredPosition;

        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.onGameOver += TriggerGameOverSequence;
        }
    }

    void OnDestroy()
    {
        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.onGameOver -= TriggerGameOverSequence;
        }
    }

    private void TriggerGameOverSequence()
    {
        StartCoroutine(PlayCameraShutterEffect());
    }

    private IEnumerator PlayCameraShutterEffect()
    {
        // TODO: fix this 
        // close shutter effect
        // float t = 0f;
        // while (t < 1f)
        // {
        //     t += Time.unscaledDeltaTime / shutterSpeed;
        //     float smoothStep = Mathf.SmoothStep(0f, 1f, t);

        //     if (topShutter != null)
        //         topShutter.anchoredPosition = Vector2.Lerp(topShutterStartPos, Vector2.zero, smoothStep);
            
        //     if (bottomShutter != null)
        //         bottomShutter.anchoredPosition = Vector2.Lerp(bottomShutterStartPos, Vector2.zero, smoothStep);

        //     yield return null;
        // }

        if (topShutter != null) topShutter.anchoredPosition = Vector2.zero;
        if (bottomShutter != null) bottomShutter.anchoredPosition = Vector2.zero;

        // camera flash effect 
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
            
            float flashT = 0f;
            while (flashT < 1f)
            {
                flashT += Time.unscaledDeltaTime / flashDuration;
                float alpha = Mathf.Lerp(1f, 0f, flashT);
                flashImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            flashImage.gameObject.SetActive(false);
        }

        // Fade in game over text 
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            
            float textT = 0f;
            while (textT < 1f)
            {
                textT += Time.unscaledDeltaTime / textFadeSpeed;
                float alpha = Mathf.Lerp(0f, 1f, textT);
                gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, alpha);
                yield return null;
            }
        }

        
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (flashImage != null) flashImage.gameObject.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (topShutter != null) topShutter.gameObject.SetActive(false);
        if (bottomShutter != null) bottomShutter.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}