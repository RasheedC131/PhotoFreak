using UnityEngine;

public class ConsumableFilm : ConsumableItem
{
    [Header("Film Settings")]
    [SerializeField] private int filmCounterAddition = 1; 
    [SerializeField] private int maxUses = 1; 

    private int currentUses; 

    protected override void Awake()
    {
        base.Awake(); 
        currentUses = maxUses; 
    }

    public override void OnUse()
    {
        if (currentUses > 0)
        {
            currentUses --; 
            Debug.Log($"Used Film. Uses left: {currentUses}");

            if (currentUses <= 0)
            {
                FindObjectOfType<PlayerInventory>().RemoveCurrentItem(); 
                Destroy(gameObject); 
            }
        }
    }
}
