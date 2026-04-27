using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HUDManager : MonoBehaviour
{
    private Canvas myCanvas;

    void Start()
    {
        myCanvas = GetComponent<Canvas>();

        if (GlobalGameState.Instance != null) GlobalGameState.Instance.onGameOver += HideHUD;
        
    }

    void OnDestroy()
    {
    
        if (GlobalGameState.Instance != null) GlobalGameState.Instance.onGameOver -= HideHUD;
        
    }

    private void HideHUD()
    {
        if (myCanvas != null) myCanvas.enabled = false;
    }

    public void SetHUDVisible(bool visible)
    {
        if (myCanvas != null) myCanvas.enabled = visible;
    }
}