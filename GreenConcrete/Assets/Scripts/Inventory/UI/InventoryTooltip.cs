using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public Image iconImage;
    public RectTransform tooltipPanel;

    [Header("Settings")]
    public Vector2 offset = new Vector2(10, -10);

    private bool isVisible = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (tooltipPanel == null)
        {
            tooltipPanel = GetComponent<RectTransform>();
        }

        HideTooltip();
    }

    void Update()
    {
        if (isVisible)
        {
            UpdatePosition();
        }
    }

    public void ShowTooltip(InventoryItem item)
    {
        if (item == null || item.definition == null)
            return;

        if (nameText != null)
        {
            nameText.text = item.definition.itemName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = item.definition.description;
        }

        if (statsText != null)
        {
            string stats = $"Size: {item.Width}x{item.Height}\n";
            stats += $"Weight: {item.definition.weight:F2} kg\n";
            stats += $"Value: {item.definition.value}";

            if (item.definition.isStackable)
            {
                stats += $"\nStack: {item.currentStackSize}/{item.definition.maxStackSize}";
            }

            statsText.text = stats;
        }

        if (iconImage != null && item.definition.icon != null)
        {
            iconImage.sprite = item.definition.icon;
            iconImage.enabled = true;
        }

        gameObject.SetActive(true);
        isVisible = true;
        UpdatePosition();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
        isVisible = false;
    }

    void UpdatePosition()
    {
        if (tooltipPanel == null)
            return;

        Vector2 mousePosition = Input.mousePosition;
        tooltipPanel.position = mousePosition + offset;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Vector2 tooltipSize = tooltipPanel.sizeDelta;

            Vector2 adjustedPosition = tooltipPanel.position;

            if (adjustedPosition.x + tooltipSize.x > screenSize.x)
            {
                adjustedPosition.x = screenSize.x - tooltipSize.x - 10;
            }

            if (adjustedPosition.y - tooltipSize.y < 0)
            {
                adjustedPosition.y = tooltipSize.y + 10;
            }

            tooltipPanel.position = adjustedPosition;
        }
    }
}
