using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI stackText;

    private InventoryItem item;
    private InventoryGridUI parentGrid;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
    }

    public void SetItem(InventoryItem inventoryItem, InventoryGridUI grid)
    {
        item = inventoryItem;
        parentGrid = grid;

        if (iconImage != null && item.definition.icon != null)
        {
            iconImage.sprite = item.definition.icon;
            iconImage.enabled = true;
        }

        if (stackText != null)
        {
            if (item.definition.isStackable && item.currentStackSize > 1)
            {
                stackText.text = item.currentStackSize.ToString();
                stackText.enabled = true;
            }
            else
            {
                stackText.enabled = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || parentGrid == null)
            return;

        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        InventoryDragHandler.Instance?.StartDrag(item, parentGrid, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null)
            return;

        rectTransform.anchoredPosition += eventData.delta;
        InventoryDragHandler.Instance?.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        InventoryDragHandler.Instance?.EndDrag(eventData.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.ShowTooltip(item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    public InventoryItem GetItem()
    {
        return item;
    }
}
