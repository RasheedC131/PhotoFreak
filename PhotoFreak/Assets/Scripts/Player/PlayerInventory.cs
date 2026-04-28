using UnityEngine;
using System; 
using System.Collections;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public event Action<Sprite, string> OnSlotChanged; 
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

    [Header("UI References (World Space)")]
    [SerializeField] private TextMeshProUGUI interactPromptText; 
    [SerializeField] private float promptHeightOffset = 0.5f;

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
            
            StartCoroutine(InitializeCameraUI(camTool.itemIcon, camTool.itemName));
        }
        else
        {
            Debug.LogError("Camera Object is missing an IEquippable script!");
        }

        inputManager.OnShoot += UseCurrentItem;       
        inputManager.OnZoom += CycleInventory;        
        inputManager.OnInteract += HandleInteraction; 
    }

    private IEnumerator InitializeCameraUI(Sprite camIcon, string name)
    {
        yield return new WaitForEndOfFrame();
        OnSlotChanged?.Invoke(camIcon, name);
    }

    void Update()
    {
        CheckForInteractable();
    }

private void CheckForInteractable()
    {
        if (interactPromptText == null) return; 

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IInteractable interactableFixture = hit.collider.GetComponent<IInteractable>();
            if (interactableFixture != null)
            {
                ShowPrompt(hit, $"[E]) {interactableFixture.promptText}", interactableFixture.promptLocation);
                return;
            }

            IEquippable itemOnGround = hit.collider.GetComponent<IEquippable>();
            if (itemOnGround != null)
            {
                ShowPrompt(hit, $"[E]) {itemOnGround.itemName}", null);
                return; 
            }
        }

        interactPromptText.gameObject.SetActive(false);
    }

    private void ShowPrompt(RaycastHit hit, string textToShow, Transform customLocation)
    {
        interactPromptText.text = textToShow;

        Vector3 basePosition = customLocation != null 
            ? customLocation.position 
            : hit.collider.transform.position + (Vector3.up * promptHeightOffset);

        Vector3 directionToCamera = (playerCamera.transform.position - basePosition).normalized;
        interactPromptText.transform.position = basePosition + (directionToCamera * 0.15f);
        interactPromptText.transform.rotation = Quaternion.LookRotation(interactPromptText.transform.position - playerCamera.transform.position);
        interactPromptText.gameObject.SetActive(true);
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
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IInteractable interactableFixture = hit.collider.GetComponent<IInteractable>();
            if (interactableFixture != null)
            {
                interactableFixture.Interact();
                return; 
            }

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
                Debug.Log("Inventory full!");
                return;
            }
        }

        SwitchToSlot(targetSlot);
        inventorySlots[targetSlot] = newItem;
        newItem.OnPickup(handHoldPos);
        newItem.OnEquip();
        OnSlotChanged?.Invoke(newItem.itemIcon, newItem.itemName);
    }

    public void RemoveCurrentItem()
    {
        if (inventorySlots[currentSlotIndex] == null) return;
        inventorySlots[currentSlotIndex].OnUnequip(); 
        inventorySlots[currentSlotIndex] = null;
        OnSlotChanged?.Invoke(null, "");
        SwitchToSlot(0); 
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
            OnSlotChanged?.Invoke(null, "");
            SwitchToSlot(0); 
        }
    }

    private void SwitchToSlot(int newSlot)
    {
        if (currentSlotIndex == newSlot) return;

        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnUnequip();
        currentSlotIndex = newSlot;
        if (inventorySlots[currentSlotIndex] != null) inventorySlots[currentSlotIndex].OnEquip();

        if (inventorySlots[currentSlotIndex] != null)
        {
            OnSlotChanged?.Invoke(inventorySlots[currentSlotIndex].itemIcon, inventorySlots[currentSlotIndex].itemName);
        }
        else
        {
            OnSlotChanged?.Invoke(null, "");
        }
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