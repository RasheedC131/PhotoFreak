using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform topShutter;
    [SerializeField] private RectTransform bottomShutter;
    [SerializeField] private Image flashImage;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Animation Settings")]
    [SerializeField] private float shutterSpeed = 0.2f;   // How fast the shutters close
    [SerializeField] private float flashDuration = 0.5f;  // How long the flash fades out
    [SerializeField] private float textFadeSpeed = 1.0f;  // How fast the text fades in

    private Vector2 topShutterStartPos;
    private Vector2 bottomShutterStartPos;

    void Start()
    {
        // 1. Hide the text and flash, set their alpha to 0
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

        // 2. Store the starting positions of the shutters (off-screen)
        if (topShutter != null) topShutterStartPos = topShutter.anchoredPosition;
        if (bottomShutter != null) bottomShutterStartPos = bottomShutter.anchoredPosition;

        // 3. Subscribe to the Game Over event
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
        // Close the Shutters
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / shutterSpeed;
            
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            if (topShutter != null)
                topShutter.anchoredPosition = Vector2.Lerp(topShutterStartPos, Vector2.zero, smoothStep);
            
            if (bottomShutter != null)
                bottomShutter.anchoredPosition = Vector2.Lerp(bottomShutterStartPos, Vector2.zero, smoothStep);

            yield return null;
        }

        // Snap them exactly to zero just to be safe
        if (topShutter != null) topShutter.anchoredPosition = Vector2.zero;
        if (bottomShutter != null) bottomShutter.anchoredPosition = Vector2.zero;

        // Camera Flash
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
            
            float flashT = 0f;
            while (flashT < 1f)
            {
                flashT += Time.unscaledDeltaTime / flashDuration;
                
                // Fade from full white (alpha 1) to invisible (alpha 0)
                float alpha = Mathf.Lerp(1f, 0f, flashT);
                flashImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            flashImage.gameObject.SetActive(false);
        }

        // Text fade in 
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
    }
}