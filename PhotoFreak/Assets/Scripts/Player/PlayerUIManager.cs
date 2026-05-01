using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject photoCanvas;
    [SerializeField] private GameObject photoReviewCanvas;
    [SerializeField] private GameObject gameUICanvas;
    [SerializeField] private GameObject interactionCanvas;
    [SerializeField] private GameObject freakMeterUI;

    [SerializeField] private UnityEngine.UI.Slider developmentBar;

    private StarRating star;
    private ReviewDetails details;

    void Awake()
    {
        star = photoReviewCanvas.GetComponentInChildren<StarRating>();
        details = photoReviewCanvas.GetComponentInChildren<ReviewDetails>();
    }

    void Start()
    {
        if (GlobalGameState.Instance != null)
            GlobalGameState.Instance.onGameOver += HideHUD;
    }

    void OnDestroy()
    {
        if (GlobalGameState.Instance != null)
            GlobalGameState.Instance.onGameOver -= HideHUD;
    }

    public void UpdateCanvasState(bool raised)
    {
        SetGameUIVisible(!raised);
        photoCanvas.SetActive(raised);
        photoReviewCanvas.SetActive(false);
    }

    public void DisplayResults(ScoreParameters data)
    {
        RawImage capturedPhoto = photoReviewCanvas.GetComponentInChildren<RawImage>();
        capturedPhoto.texture = data.currentPhoto;

        star.DisplayStars(Mathf.RoundToInt(data.result));
        details.FillDetails(data);

        gameUICanvas.SetActive(false);
        photoCanvas.SetActive(false);
        photoReviewCanvas.SetActive(true);
    }

    public void SetHUDVisible(bool visible)
    {
        SetGameUIVisible(visible);
    }

    private void SetGameUIVisible(bool visible)
    {
        if (gameUICanvas == null) return;

        // DisplayResults() deactivates the whole canvas — re-enable the parent
        // before showing children, otherwise they stay hidden despite being active.
        if (visible) gameUICanvas.SetActive(true);

        foreach (Transform child in gameUICanvas.transform)
        {
            if (freakMeterUI != null && child.gameObject == freakMeterUI) continue;

            child.gameObject.SetActive(visible);
        }
    }

    private void HideHUD()
    {
        SetHUDVisible(false);
    }

    public void UpdateDevelopmentBar(float value)
    {
        developmentBar.value = Mathf.Clamp01(value);
    }
}
