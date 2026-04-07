using UnityEngine;

public class DrinkItem : ConsumableItem
{
    [Header("Drink Settings")]
    [SerializeField] private float freakMeterReduction = 10f;
    [SerializeField] private int maxSips = 3; 
    
    private int currentSips;

    protected override void Awake()
    {
        base.Awake(); 
        currentSips = maxSips;
    }

    public override void OnUse()
    {
        if (currentSips > 0)
        {
            currentSips--; 
            Debug.Log($"Drank Item");
            
            
            if (currentSips <= 0)
            {
                Debug.Log($"The {itemName} is empty");
                Destroy(gameObject); 
            }
        }
    }
}