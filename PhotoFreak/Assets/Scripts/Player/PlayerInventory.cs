using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    // Active slot changed — sends the equipped item's icon and name
    public event Action<Sprite, string> OnSlotChanged;
    // A slot's icon changed — sends slot index and new sprite
    public event Action<int, Sprite> OnSlotUpdated;
    // Which slot index is now active — for hotbar highlighting
    public event Action<int> OnActiveSlotChanged;

    [Header("Inventory Settings")]
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

    private const int InventorySize = 2;
    private IEquippable[] inventorySlots = new IEquippable[InventorySize];
    private int currentSlotIndex = 0;

    void Start()
    {
        if (inputManager == null) inputManager = GetComponent<InputManager>();

        IEquippable camTool = photoCameraObj != null ? photoCameraObj.GetComponent<IEquippable>() : null;
        if (camTool != null)
        {
            inventorySlots[0] = camTool;
            camTool.OnEquip();
            StartCoroutine(InitializeUI());
        }
        else
        {
            Debug.LogError("Camera Object is missing an IEquippable script!");
        }

        inputManager.OnShoot    += UseCurrentItem;
        inputManager.OnZoom     += CycleInventory;
        inputManager.OnInteract += HandleInteraction;
    }

    private IEnumerator InitializeUI()
    {
        yield return new WaitForEndOfFrame();
        // Populate both slot icons on startup
        for (int i = 0; i < InventorySize; i++)
            OnSlotUpdated?.Invoke(i, inventorySlots[i]?.itemIcon);

        OnSlotChanged?.Invoke(inventorySlots[0]?.itemIcon, inventorySlots[0]?.itemName ?? "");
        OnActiveSlotChanged?.Invoke(0);

        // Hide any items that aren't in the active slot
        for (int i = 0; i < InventorySize; i++)
        {
            if (i != currentSlotIndex && inventorySlots[i] != null)
                inventorySlots[i].gameObject.SetActive(false);
        }
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
            : hit.collider.transform.position + Vector3.up * promptHeightOffset;

        Vector3 dirToCamera = (playerCamera.transform.position - basePosition).normalized;
        interactPromptText.transform.position = basePosition + dirToCamera * 0.15f;
        interactPromptText.transform.rotation = Quaternion.LookRotation(
            interactPromptText.transform.position - playerCamera.transform.position);
        interactPromptText.gameObject.SetActive(true);
    }

    private void CycleInventory(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) < 0.01f) return;
        if (inventorySlots[currentSlotIndex] != null && inventorySlots[currentSlotIndex].isInUse) return;

        // Toggle between slot 0 and slot 1
        int newSlot = currentSlotIndex == 0 ? 1 : 0;
        SwitchToSlot(newSlot);
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
        // Always place picked-up items in slot 1 (slot 0 is the camera)
        int targetSlot = 1;

        if (inventorySlots[targetSlot] != null)
        {
            if (inventorySlots[targetSlot].isDroppable)
                DropSlot(targetSlot);
            else
            {
                Debug.Log("Slot 1 is occupied and not droppable.");
                return;
            }
        }

        inventorySlots[targetSlot] = newItem;
        newItem.OnPickup(handHoldPos);

        if (currentSlotIndex == targetSlot)
            newItem.OnEquip();
        else
            newItem.gameObject.SetActive(false); // Keep inactive items hidden until switched to

        OnSlotUpdated?.Invoke(targetSlot, newItem.itemIcon);

        if (currentSlotIndex == targetSlot)
            OnSlotChanged?.Invoke(newItem.itemIcon, newItem.itemName);
    }

    public void RemoveCurrentItem()
    {
        if (inventorySlots[currentSlotIndex] == null) return;
        inventorySlots[currentSlotIndex].OnUnequip();
        inventorySlots[currentSlotIndex] = null;
        OnSlotUpdated?.Invoke(currentSlotIndex, null);
        OnSlotChanged?.Invoke(null, "");
        SwitchToSlot(0);
    }

    private void DropCurrentItem()
    {
        DropSlot(currentSlotIndex);
    }

    private void DropSlot(int slotIndex)
    {
        if (inventorySlots[slotIndex] == null) return;

        IEquippable item = inventorySlots[slotIndex];
        if (!item.isDroppable || item.isInUse) return;

        if (slotIndex == currentSlotIndex) item.OnUnequip();
        item.OnDrop();
        inventorySlots[slotIndex] = null;

        OnSlotUpdated?.Invoke(slotIndex, null);
        if (slotIndex == currentSlotIndex)
        {
            OnSlotChanged?.Invoke(null, "");
            SwitchToSlot(0);
        }
    }

    private void SwitchToSlot(int newSlot)
    {
        if (currentSlotIndex == newSlot) return;

        if (inventorySlots[currentSlotIndex] != null)
            inventorySlots[currentSlotIndex].OnUnequip();

        currentSlotIndex = newSlot;

        if (inventorySlots[currentSlotIndex] != null)
            inventorySlots[currentSlotIndex].OnEquip();

        Sprite icon = inventorySlots[currentSlotIndex]?.itemIcon;
        string name = inventorySlots[currentSlotIndex]?.itemName ?? "";
        OnSlotChanged?.Invoke(icon, name);
        OnActiveSlotChanged?.Invoke(currentSlotIndex);
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnShoot    -= UseCurrentItem;
            inputManager.OnZoom     -= CycleInventory;
            inputManager.OnInteract -= HandleInteraction;
        }
    }
}
