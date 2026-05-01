using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private TMP_Text itemName;

    [Header("Top Slot")]
    [SerializeField] private Image topBackground;
    [SerializeField] private Image topIcon;

    [Header("Bottom Slot")]
    [SerializeField] private Image bottomBackground;
    [SerializeField] private Image bottomIcon;


    [Header("Colors")]
    [SerializeField] private Color activeColor   = new Color(.56f, .48f, .77f, .5f);
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Top Slot Visibility")]
    [SerializeField] private float hideDelay = 1.5f;

    [Header("Item Name Text")]
    [SerializeField] private float fadeInTime  = 0.2f;
    [SerializeField] private float fadeOutTime = 0.8f;

    // Cached sprites for both inventory slots
    private Sprite[] slotSprites = new Sprite[2];
    private int activeIndex = 0;

    private Coroutine hideCoroutine;
    private Coroutine textFadeCoroutine;

    void Start()
    {
        if (playerInventory != null)
        {
            playerInventory.OnSlotChanged       += UpdateItemName;
            playerInventory.OnSlotUpdated       += UpdateSlotSprite;
            playerInventory.OnActiveSlotChanged += UpdateActiveIndex;
        }

        if (itemName != null) SetTextAlpha(0f);
        RefreshDisplay();
        SetTopSlotVisible(false);
    }

    private void UpdateItemName(Sprite icon, string name)
    {
        if (itemName == null) return;
        itemName.text = name;

        if (textFadeCoroutine != null) StopCoroutine(textFadeCoroutine);
        textFadeCoroutine = StartCoroutine(FadeTextInThenOut());
    }

    private IEnumerator FadeTextInThenOut()
    {
        // Fade in
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            SetTextAlpha(Mathf.Clamp01(t / fadeInTime));
            yield return null;
        }
        SetTextAlpha(1f);

        // Hold while the top slot is visible, then fade out
        yield return new WaitForSeconds(hideDelay);

        // Fade out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            SetTextAlpha(Mathf.Clamp01(1f - (t / fadeOutTime)));
            yield return null;
        }
        SetTextAlpha(0f);
        textFadeCoroutine = null;
    }

    private void SetTextAlpha(float alpha)
    {
        if (itemName == null) return;
        Color c = itemName.color;
        c.a = alpha;
        itemName.color = c;
    }

    private void UpdateSlotSprite(int slotIndex, Sprite newSprite)
    {
        slotSprites[slotIndex] = newSprite;
        RefreshDisplay();
    }

    private void UpdateActiveIndex(int index)
    {
        activeIndex = index;
        RefreshDisplay();

        // Show the top slot briefly when switching
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        SetTopSlotVisible(true);
        hideCoroutine = StartCoroutine(HideTopSlotAfterDelay());
    }

    private IEnumerator HideTopSlotAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        SetTopSlotVisible(false);
        hideCoroutine = null;
    }

    private void SetTopSlotVisible(bool visible)
    {
        if (topBackground != null) topBackground.gameObject.SetActive(visible);
        if (topIcon != null)       topIcon.gameObject.SetActive(visible);
    }

    private void RefreshDisplay()
    {
        int otherIndex = 1 - activeIndex;

        // Bottom = active item
        SetIcon(bottomIcon, slotSprites[activeIndex]);
        bottomBackground.color = activeColor;

        // Top = the other item
        SetIcon(topIcon, slotSprites[otherIndex]);
        topBackground.color = inactiveColor;
    }

    private void SetIcon(Image icon, Sprite sprite)
    {
        if (icon == null) return;
        icon.sprite = sprite;
        icon.color  = sprite != null ? Color.white : Color.clear;
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnSlotChanged       -= UpdateItemName;
            playerInventory.OnSlotUpdated       -= UpdateSlotSprite;
            playerInventory.OnActiveSlotChanged -= UpdateActiveIndex;
        }
    }
}
