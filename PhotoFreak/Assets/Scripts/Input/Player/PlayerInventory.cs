using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Define your slots here
    private enum InventorySlot
    {
        CAMERA = 0,
        ITEM = 1
    }

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 2; 

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform handHoldPos; // Parent object for held items
    [SerializeField] private Camera playerCamera;   // Origin of Raycast
    
    [Header("Inventory Config")]
    [SerializeField] private GameObject photoCameraObj; 
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactionRange = 3f;

    private IEquippable[] inventorySlots;
    
    private int currentSlotIndex = (int)InventorySlot.CAMERA; 

    void Start()
    {
        if (inputManager == null) inputManager = GetComponent<InputManager>();

        // ensure size is at least 2 (Camera + 1 Item slot)
        if (inventorySize < 2) inventorySize = 2;
        inventorySlots = new IEquippable[inventorySize];

        IEquippable camTool = photoCameraObj.GetComponent<IEquippable>();
        if (camTool != null)
        {
            inventorySlots[(int)InventorySlot.CAMERA] = camTool;
            camTool.OnEquip(); // Start with camera equipped
        }
        else
        {
            Debug.LogError("Camera Object is missing the IEquippable script!");
        }

        inputManager.OnShoot += UseCurrentItem;         // Left Click
        inputManager.OnZoom += CycleInventory;          // Scroll Wheel
        inputManager.OnInteract += HandleInteraction;   // E Key
    }

    private void CycleInventory(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) < 0.01f) return;

        if (inventorySlots[currentSlotIndex] != null)
        {
            inventorySlots[currentSlotIndex].OnUnequip();
        }

        int direction = scrollValue > 0 ? 1 : -1;

        currentSlotIndex = (currentSlotIndex + direction);

        if (currentSlotIndex >= inventorySize) currentSlotIndex = 0;
        if (currentSlotIndex < 0) currentSlotIndex = inventorySize - 1;

        if (inventorySlots[currentSlotIndex] != null)
        {
            inventorySlots[currentSlotIndex].OnEquip();
        }
        else
        {
            Debug.Log($"Switched to Empty Slot {currentSlotIndex}");
        }
    }

    private void UseCurrentItem()
    {
        if (inventorySlots[currentSlotIndex] != null)
        {
            inventorySlots[currentSlotIndex].OnUse();
        }
    }

    private void HandleInteraction()
    {

        // if holding an item drop it 
        if (currentSlotIndex != (int)InventorySlot.CAMERA && inventorySlots[currentSlotIndex] != null)
        {
            DropItem();
        }

        // if camera is equipped or free hand 
        else
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            DrinkItem newItem = hit.collider.GetComponent<DrinkItem>();
            
            if (newItem != null)
            {
                int targetSlot = currentSlotIndex;

                if (currentSlotIndex == (int)InventorySlot.CAMERA)
                {
                    targetSlot = (int)InventorySlot.ITEM;

                    // TODO: maybe added swapping slots 
                }

                // Switch to that slot visually before picking up
                SwitchToSlot(targetSlot);
                
                // Perform the pickup
                PickupItem(newItem, targetSlot);
            }
        }
    }

    private void PickupItem(DrinkItem item, int slotIndex)
    {
        // swapping slots 
        if (inventorySlots[slotIndex] != null)
        {
            DrinkItem oldItem = inventorySlots[slotIndex] as DrinkItem;
            if (oldItem != null)
            {
                oldItem.OnUnequip();
                oldItem.OnDrop(); // Make sure DrinkItem has this method
                oldItem.transform.SetParent(null);
            }
        }

        inventorySlots[slotIndex] = item;

        // Visuals
        item.transform.SetParent(handHoldPos);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        item.OnPickup(); 
        item.OnEquip();    
    }

    private void DropItem()
    {
        DrinkItem item = inventorySlots[currentSlotIndex] as DrinkItem;
        
        if (item != null)
        {
            item.OnUnequip();
            item.OnDrop(); 
            item.transform.SetParent(null);
        }

        inventorySlots[currentSlotIndex] = null; 
    }

    private void SwitchToSlot(int newSlot)
    {
        if (currentSlotIndex == newSlot) return;

        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnUnequip();

        currentSlotIndex = newSlot;

        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnEquip();
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnShoot -= UseCurrentItem;
            inputManager.OnZoom -= CycleInventory;
            inputManager.OnInteract -= HandleInteraction;
        }
    }
}