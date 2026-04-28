using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject photoCanvas;
    [SerializeField] private GameObject photoReviewCanvas;
    [SerializeField] private GameObject gameUICanvas;
    [SerializeField] private GameObject interactionCanvas;

    private StarRating star;

    void Awake()
    {
        star = photoReviewCanvas.GetComponentInChildren<StarRating>();
    }

    public void UpdateCanvasState(bool raised)
    {
        gameUICanvas.SetActive(!raised);
        photoCanvas.SetActive(raised);

        photoReviewCanvas.SetActive(false);
    }

    public void DisplayResults(ScoreParameters data)
    {
        RawImage capturedPhoto = photoReviewCanvas.GetComponentInChildren<RawImage>();
        capturedPhoto.texture = data.currentPhoto;

        star.DisplayStars(Mathf.RoundToInt(data.result));

        gameUICanvas.SetActive(false);
        photoCanvas.SetActive(false);

        photoReviewCanvas.SetActive(true);
    }

    public void ToggleUI(bool active)
    {
        gameUICanvas.SetActive(active);
        Debug.Log("boop");
    }
}
