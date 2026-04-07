using UnityEngine;
using System; 
using System.Collections; 

public class PlayerInventory : MonoBehaviour
{
    public event Action<int> OnSlotChanged; 
    public event Action<int, Sprite> OnSlotUpdated;

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 3; 
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

        IEquippable camTool = photoCameraObj.GetComponent<IEquippable>();
        if (camTool != null)
        {
            inventorySlots[0] = camTool;
            camTool.OnEquip(); 
            StartCoroutine(InitializeCameraUI(camTool.itemIcon));
        }
        else Debug.LogError("Camera Object is missing an IEquippable script!");

        inputManager.OnShoot += UseCurrentItem;       
        inputManager.OnZoom += CycleInventory;        
        inputManager.OnInteract += HandleInteraction; 
    }

    private IEnumerator InitializeCameraUI(Sprite camIcon)
    {
        yield return new WaitForEndOfFrame(); 
        OnSlotUpdated?.Invoke(0, camIcon);
        OnSlotChanged?.Invoke(0);
    }

    public void RemoveCurrentItem()
    {
        inventorySlots[currentSlotIndex] = null;         
        OnSlotUpdated?.Invoke(currentSlotIndex, null);   
        SwitchToSlot(0);                               
    }

    private void CycleInventory(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) < 0.01f) return;
        if (inventorySlots[currentSlotIndex] != null && inventorySlots[currentSlotIndex].isInUse) return; 

        int direction = scrollValue > 0 ? 1 : -1;
        int newSlotIndex = currentSlotIndex + direction;

        if (newSlotIndex >= inventorySize) newSlotIndex = 0;
        if (newSlotIndex < 0) newSlotIndex = inventorySize - 1;

        SwitchToSlot(newSlotIndex);
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
        OnSlotUpdated?.Invoke(targetSlot, newItem.itemIcon);
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
            OnSlotUpdated?.Invoke(currentSlotIndex, null);
            SwitchToSlot(0); 
        }
    }

    private void SwitchToSlot(int newSlot)
    {
        if (currentSlotIndex == newSlot) return;
        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnUnequip();
        currentSlotIndex = newSlot;
        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnEquip();
        OnSlotChanged?.Invoke(currentSlotIndex);
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