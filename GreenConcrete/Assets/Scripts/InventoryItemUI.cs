using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image iconImage;
    public TextMeshProUGUI stackText;
    public Image backgroundImage;

    [Header("Visual")]
    public Color normalColor = Color.white;
    public Color dragColor = new Color(1f, 1f, 1f, 0.6f);

    private InventoryItem item;
    private InventoryGridUI gridUI;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private int originalGridX;
    private int originalGridY;
    private bool isDragging;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(InventoryItem inventoryItem, InventoryGridUI grid)
    {
        item = inventoryItem;
        gridUI = grid;
        canvas = GetComponentInParent<Canvas>();

        if (iconImage != null && item.definition.icon != null)
        {
            iconImage.sprite = item.definition.icon;
        }

        if (stackText != null)
        {
            if (item.definition.isStackable && item.stackCount > 1)
            {
                stackText.text = item.stackCount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }

        UpdateSize();
    }

    void UpdateSize()
    {
        int cellSize = gridUI.GetCellSize();
        int cellSpacing = gridUI.GetCellSpacing();

        float width = item.Width * cellSize + (item.Width - 1) * cellSpacing;
        float height = item.Height * cellSize + (item.Height - 1) * cellSpacing;

        rectTransform.sizeDelta = new Vector2(width, height);
    }

    public void SetGridPosition(int x, int y)
    {
        originalGridX = x;
        originalGridY = y;

        Vector2 worldPos = gridUI.GridToWorldPosition(x, y);
        rectTransform.position = worldPos;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryDragHandler.Instance.StartDrag(item, this, gridUI);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        Vector2Int gridPos = gridUI.ScreenToGridPosition(eventData.position);
        bool canPlace = gridUI.inventoryGrid.CanPlaceItem(item, gridPos.x, gridPos.y);
        gridUI.HighlightCells(gridPos.x, gridPos.y, item.Width, item.Height, canPlace);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Vector2Int gridPos = gridUI.ScreenToGridPosition(eventData.position);
        
        if (gridUI.inventoryGrid.CanPlaceItem(item, gridPos.x, gridPos.y))
        {
            gridUI.inventoryGrid.PlaceItem(item, gridPos.x, gridPos.y);
            SetGridPosition(gridPos.x, gridPos.y);
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        gridUI.ResetCellColors();
        InventoryDragHandler.Instance.EndDrag();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.ShowTooltip(item, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    void Update()
    {
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    void RotateItem()
    {
        if (!item.definition.canRotate) return;

        item.Rotate();
        UpdateSize();

        Vector2Int gridPos = gridUI.ScreenToGridPosition(rectTransform.position);
        bool canPlace = gridUI.inventoryGrid.CanPlaceItem(item, gridPos.x, gridPos.y);
        gridUI.HighlightCells(gridPos.x, gridPos.y, item.Width, item.Height, canPlace);
    }
}
