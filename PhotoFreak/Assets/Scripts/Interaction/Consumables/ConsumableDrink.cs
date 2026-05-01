using UnityEngine;

public class ConsumableDrink : ConsumableItem
{
    [Header("Drink Settings")]
    [SerializeField] private float freakMeterReduction = 20f;
    [SerializeField] private int maxSips = 3;

    private int currentSips;
    private FreakMeter freakMeter;

    protected override void Awake()
    {
        base.Awake();
        currentSips = maxSips;
        freakMeter  = FindObjectOfType<FreakMeter>();

        if (freakMeter == null)
            Debug.LogWarning("[ConsumableDrink] No FreakMeter found in scene.");
    }

    public override void OnUse()
    {
        if (currentSips <= 0) return;

        currentSips--;

        // Reduce freak level 
        if (freakMeter != null)
            freakMeter.ReduceFreak(freakMeterReduction);

        Debug.Log($"[ConsumableDrink] Drank {itemName}. Uses left: {currentSips}");

        if (currentSips <= 0)
        {
            Debug.Log($"[ConsumableDrink] {itemName} is empty.");
            FindObjectOfType<PlayerInventory>()?.RemoveCurrentItem();
            Destroy(gameObject);
        }
    }
}
