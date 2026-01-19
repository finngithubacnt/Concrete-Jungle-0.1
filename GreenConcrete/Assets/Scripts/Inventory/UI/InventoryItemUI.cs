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

    [Header("Drag Settings")]
    public int cellSize = 50;
    public int cellSpacing = 2;

    private InventoryItem item;
    private InventoryGridUI parentGrid;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private bool isDragging = false;

     void Awake()
    {
        Debug.Log($"InventoryItemUI Awake on {gameObject.name}");
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("  Created CanvasGroup");
        }

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                Debug.Log("  Auto-assigned backgroundImage from GetComponent");
            }
            else
            {
                Debug.LogError("  NO Image component found on root! This will break dragging!");
            }
        }
        
        Debug.Log($"  Awake complete: backgroundImage={backgroundImage != null}, iconImage={iconImage != null}, stackText={stackText != null}, canvas={canvas != null}");
    }

    public void SetItem(InventoryItem inventoryItem, InventoryGridUI grid)
    {
        item = inventoryItem;
        parentGrid = grid;

        Debug.Log($"SetItem: {item.definition.itemName} on {gameObject.name}");

        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            Debug.Log($"  Background Image raycastTarget SET TO TRUE. Current value: {backgroundImage.raycastTarget}");
        }
        else
        {
            Debug.LogError($"  ERROR: backgroundImage is NULL! Cannot enable raycast target!");
        }

        if (iconImage != null && item.definition.icon != null)
        {
            iconImage.sprite = item.definition.icon;
            iconImage.enabled = true;
            iconImage.raycastTarget = false;
            Debug.Log($"  Icon set, raycastTarget disabled");
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

    void Update()
    {
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"★★★ OnBeginDrag called on {gameObject.name} ★★★");
        
        if (item == null || parentGrid == null)
        {
            Debug.LogWarning($"  OnBeginDrag ABORTED: item={item}, parentGrid={parentGrid}");
            return;
        }

        Debug.Log($"  Item: {item.definition.itemName}");
        
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        InventoryGrid gridData = parentGrid.GetGrid();
        if (gridData != null)
        {
            gridData.RemoveItem(item);
            Debug.Log($"  Removed item from grid");
        }

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || item == null || parentGrid == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        Vector2Int? gridPos = parentGrid.GetGridPositionFromScreenPoint(eventData.position);

        if (gridPos.HasValue)
        {
            InventoryGrid gridData = parentGrid.GetGrid();
            bool canPlace = gridData.CanPlaceItem(item, gridPos.Value.x, gridPos.Value.y);
            parentGrid.HighlightCells(gridPos.Value.x, gridPos.Value.y, item.Width, item.Height, canPlace);
        }
        else
        {
            parentGrid.ResetCellColors();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Debug.Log($"★ OnEndDrag called for {item.definition.itemName}");

        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        parentGrid.ResetCellColors();

        Vector2Int? gridPos = parentGrid.GetGridPositionFromScreenPoint(eventData.position);
        InventoryGrid gridData = parentGrid.GetGrid();

        bool placed = false;

        if (gridPos.HasValue && gridData != null)
        {
            if (gridData.PlaceItem(item, gridPos.Value.x, gridPos.Value.y))
            {
                placed = true;
                Debug.Log($"  Placed at {gridPos.Value}");
            }
        }

        if (!placed && gridData != null)
        {
            Debug.Log($"  Could not place, finding new spot");
            Vector2Int? originalPos = gridData.FindSpaceForItem(item, false);
            if (originalPos.HasValue)
            {
                gridData.PlaceItem(item, originalPos.Value.x, originalPos.Value.y);
                Debug.Log($"  Placed at alternate position {originalPos.Value}");
            }
            else
            {
                Debug.LogWarning($"  No space found, returning to inventory");
                PlayerInventory.Instance?.AddItem(item);
            }

            ReturnToOriginalPosition();
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.onInventoryChanged?.Invoke();
        }
    }

    void RotateItem()
    {
        if (item != null && item.definition.canRotate)
        {
            item.Rotate();

            rectTransform.sizeDelta = new Vector2(
                item.Width * cellSize + (item.Width - 1) * cellSpacing,
                item.Height * cellSize + (item.Height - 1) * cellSpacing
            );

            Debug.Log($"  Rotated item to {item.Width}x{item.Height}");
        }
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
