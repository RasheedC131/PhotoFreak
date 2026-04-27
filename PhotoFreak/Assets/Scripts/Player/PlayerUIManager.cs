using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject photoCanvas;
    [SerializeField] private GameObject photoReviewCanvas;
    [SerializeField] private GameObject gameUICanvas;
    [SerializeField] private GameObject interactionCanvas;


    public void UpdateCanvasState(bool raised)
    {
        gameUICanvas.SetActive(!raised);
        photoCanvas.SetActive(raised);
    }

    public void DisplayResults(ScoreParameters data)
    {
        RawImage capturedPhoto = photoReviewCanvas.GetComponentInChildren<RawImage>();
        capturedPhoto.texture = data.currentPhoto;

        gameUICanvas.SetActive(false);
        photoCanvas.SetActive(false);

        photoReviewCanvas.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
