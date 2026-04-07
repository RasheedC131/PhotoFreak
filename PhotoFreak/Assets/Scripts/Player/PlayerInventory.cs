using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 3; // 0 = Camera, 1 = Item1, 2 = Item2
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform handHoldPos;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject photoCameraObj; 

    private IEquippable[] inventorySlots;
    private int currentSlotIndex = 0; 

    void Start()
    {
        if (inputManager == null) inputManager = GetComponent<InputManager>();

        inventorySlots = new IEquippable[inventorySize];

        // Initialize Camera in Slot 0
        IEquippable camTool = photoCameraObj.GetComponent<IEquippable>();
        if (camTool != null)
        {
            inventorySlots[0] = camTool;
            camTool.OnEquip(); 
        }
        else
        {
            Debug.LogError("Camera Object is missing an IEquippable script!");
        }

        inputManager.OnShoot += UseCurrentItem;       
        inputManager.OnZoom += CycleInventory;        
        inputManager.OnInteract += HandleInteraction; 
        
        // inputManager.OnDrop += HandleManualDrop; 
    }

    private void CycleInventory(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) < 0.01f) return;

        if (inventorySlots[currentSlotIndex] != null && inventorySlots[currentSlotIndex].isInUse) return; 
        

        if (inventorySlots[currentSlotIndex] != null)
            inventorySlots[currentSlotIndex].OnUnequip();

        int direction = scrollValue > 0 ? 1 : -1;
        currentSlotIndex += direction;

        if (currentSlotIndex >= inventorySize) currentSlotIndex = 0;
        if (currentSlotIndex < 0) currentSlotIndex = inventorySize - 1;

        if (inventorySlots[currentSlotIndex] != null)
            inventorySlots[currentSlotIndex].OnEquip();
    }

    private void UseCurrentItem()
    {
        inventorySlots[currentSlotIndex]?.OnUse();
    }

    private void HandleInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red, 2f);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IEquippable itemOnGround = hit.collider.GetComponent<IEquippable>();
            
            if (itemOnGround != null)
            {
                TryPickupItem(itemOnGround);
                return; 
            }
        }


        DropCurrentItem();
    }

    private void TryPickupItem(IEquippable newItem)
    {
        int targetSlot = -1;

        for (int i = 1; i < inventorySize; i++)
        {
            if (inventorySlots[i] == null)
            {
                targetSlot = i;
                break;
            }
        }

        if (targetSlot == -1)
        {
            if (inventorySlots[currentSlotIndex] != null && inventorySlots[currentSlotIndex].isDroppable)
            {
                DropCurrentItem();
                targetSlot = currentSlotIndex;
            }
            else
            {
                Debug.Log("Inventory full, and you cannot drop the Camera to swap!");
                return;
            }
        }

        SwitchToSlot(targetSlot);
        inventorySlots[targetSlot] = newItem;
        newItem.OnPickup(handHoldPos);
        newItem.OnEquip();
    }

    private void DropCurrentItem()
    {
        if (inventorySlots[currentSlotIndex] == null) return;

        IEquippable itemToDrop = inventorySlots[currentSlotIndex];

        if (itemToDrop.isDroppable && !itemToDrop.isInUse)
        {
            itemToDrop.OnUnequip();
            itemToDrop.OnDrop();
            inventorySlots[currentSlotIndex] = null;
            
            SwitchToSlot(0); 
        }
    }

    private void SwitchToSlot(int newSlot)
    {
        if (currentSlotIndex == newSlot) return;

        inventorySlots[currentSlotIndex]?.OnUnequip();
        currentSlotIndex = newSlot;
        inventorySlots[currentSlotIndex]?.OnEquip();
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