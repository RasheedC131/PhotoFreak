using UnityEngine;

public class ConsumableFilm : ConsumableItem
{
    [Header("Film Settings")]
    [SerializeField] private int filmCounterAddition = 5;
    [SerializeField] private int maxUses = 1;

    private int currentUses;
    private CameraController cameraController;

    protected override void Awake()
    {
        base.Awake();
        currentUses      = maxUses;
        cameraController = FindObjectOfType<CameraController>();

        if (cameraController == null)
            Debug.LogWarning("[ConsumableFilm] No CameraController found in scene.");
    }

    public override void OnUse()
    {
        if (currentUses <= 0) return;

        // Block use if the camera is already fully loaded
        if (cameraController != null && cameraController.IsFilmFull())
        {
            Debug.Log("[ConsumableFilm] Film is already full");
            return;
        }

        currentUses--;
        cameraController?.AddFilm(filmCounterAddition);

        Debug.Log($"[ConsumableFilm] Loaded film (+{filmCounterAddition}). Uses left: {currentUses}");

        if (currentUses <= 0)
        {
            Debug.Log($"[ConsumableFilm] {itemName} is spent.");
            FindObjectOfType<PlayerInventory>()?.RemoveCurrentItem();
            Destroy(gameObject);
        }
    }
}
