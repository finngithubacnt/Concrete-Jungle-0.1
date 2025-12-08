using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance { get; private set; }

    [Header("References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public Image iconImage;

    [Header("Settings")]
    public Vector2 offset = new Vector2(10, -10);

    private RectTransform tooltipRect;
    private Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    public void ShowTooltip(InventoryItem item, Vector2 position)
    {
        if (item == null || item.definition == null) return;

        itemNameText.text = item.definition.itemName;
        descriptionText.text = item.definition.description;

        if (iconImage != null && item.definition.icon != null)
        {
            iconImage.sprite = item.definition.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        string stats = BuildStatsText(item);
        statsText.text = stats;

        tooltipPanel.SetActive(true);

        UpdatePosition(position);
    }

    string BuildStatsText(InventoryItem item)
    {
        string stats = "";
        
        stats += $"Size: {item.definition.width}x{item.definition.height}\n";
        stats += $"Weight: {item.definition.weight:F2} kg\n";
        stats += $"Value: {item.definition.value}\n";
        stats += $"Category: {item.definition.category}\n";

        if (item.definition.isStackable)
        {
            stats += $"Stack: {item.stackCount}/{item.definition.maxStackSize}\n";
        }

        if (item.definition.isContainer)
        {
            stats += $"Container: {item.definition.containerWidth}x{item.definition.containerHeight}\n";
        }

        return stats;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    void UpdatePosition(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out localPoint
        );

        tooltipRect.anchoredPosition = localPoint + offset;

        ClampToScreen();
    }

    void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);

        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector2 adjustment = Vector2.zero;

        if (corners[2].x > canvasCorners[2].x)
        {
            adjustment.x = canvasCorners[2].x - corners[2].x;
        }
        if (corners[0].x < canvasCorners[0].x)
        {
            adjustment.x = canvasCorners[0].x - corners[0].x;
        }
        if (corners[2].y > canvasCorners[2].y)
        {
            adjustment.y = canvasCorners[2].y - corners[2].y;
        }
        if (corners[0].y < canvasCorners[0].y)
        {
            adjustment.y = canvasCorners[0].y - corners[0].y;
        }

        tooltipRect.anchoredPosition += adjustment;
    }
}
