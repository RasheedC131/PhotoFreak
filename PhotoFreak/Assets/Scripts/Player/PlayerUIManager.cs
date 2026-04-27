using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
