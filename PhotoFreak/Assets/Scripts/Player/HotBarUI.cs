using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    
    [Header("UI Elements")]
    [SerializeField] private Image[] slotBackgrounds; 
    [SerializeField] private Image[] slotIcons;    
    
    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1, 1, 1, 0.5f); 

    void Start()
    {
        if (playerInventory != null)
        {
            playerInventory.OnSlotChanged += UpdateActiveSlot;
            playerInventory.OnSlotUpdated += UpdateSlotIcon;
        }

        foreach (Image icon in slotIcons)
        {
            icon.color = Color.clear;
        }

        UpdateActiveSlot(0);
    }

    private void UpdateActiveSlot(int activeIndex)
    {
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (i == activeIndex)
                slotBackgrounds[i].color = activeColor;
            else
                slotBackgrounds[i].color = inactiveColor;
        }
    }

    private void UpdateSlotIcon(int slotIndex, Sprite newIcon)
    {
        if (newIcon != null)
        {
            slotIcons[slotIndex].sprite = newIcon;
            slotIcons[slotIndex].color = Color.white;
        }
        else
        {
            slotIcons[slotIndex].sprite = null;
            slotIcons[slotIndex].color = Color.clear; 
        }
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnSlotChanged -= UpdateActiveSlot;
            playerInventory.OnSlotUpdated -= UpdateSlotIcon;
        }
    }
}