using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private TMP_Text itemName;
    [Header("UI Elements")]
    [SerializeField] private Image[] slotBackgrounds; 
    [SerializeField] private Image[] slotIcons;    
    
    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(.56f, .48f, .77f, .5f);
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
        foreach (Image background in slotBackgrounds)
        {
            background.color = activeColor;
        }

    }
    private void UpdateActiveSlot(Sprite newIcon, string name)
    {
        slotIcons[0].sprite = newIcon;
        slotBackgrounds[0].color = activeColor;
        itemName.text = name;
        if (newIcon == null) slotIcons[0].color = inactiveColor;
        else slotIcons[0].color = Color.white;
    }   

    private void UpdateSlotIcon(int slotIndex, Sprite newIcon)
    {
        if (newIcon != null)
        {
            slotIcons[slotIndex].sprite = newIcon;
            itemName.text = name;
            slotIcons[0].color = Color.white;
            slotBackgrounds[0].color = activeColor;
        }
        else
        {
            slotIcons[slotIndex].sprite = null;
            itemName.text = "";
            slotIcons[0].color = activeColor;
            slotBackgrounds[0].color = activeColor;
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