using UnityEngine;

public class HUDPositioning : MonoBehaviour
{
    [Header("HUD Elements")]
    public RectTransform inventoryPanel;
    public RectTransform objectInfoPanel;
    public RectTransform menuButton;
    public RectTransform minimap;

    [Header("Margins")]
    public float margin = 20f;

    void Start()
    {
        PositionHUDElements();
    }

    void PositionHUDElements()
    {
        // Inventory - bottom center
        if (inventoryPanel != null)
        {
            inventoryPanel.anchorMin = new Vector2(0.3f, 0f);
            inventoryPanel.anchorMax = new Vector2(0.7f, 0.2f);
            inventoryPanel.anchoredPosition = Vector2.zero;
        }

        // Object Info Panel - right center
        if (objectInfoPanel != null)
        {
            objectInfoPanel.anchorMin = new Vector2(0.7f, 0.3f);
            objectInfoPanel.anchorMax = new Vector2(0.95f, 0.7f);
            objectInfoPanel.anchoredPosition = Vector2.zero;
        }

        // Menu Button - top right
        if (menuButton != null)
        {
            menuButton.anchorMin = new Vector2(0.85f, 0.85f);
            menuButton.anchorMax = new Vector2(0.95f, 0.95f);
            menuButton.anchoredPosition = Vector2.zero;
        }

        // Minimap - bottom left
        if (minimap != null)
        {
            minimap.anchorMin = new Vector2(0.05f, 0.05f);
            minimap.anchorMax = new Vector2(0.25f, 0.25f);
            minimap.anchoredPosition = Vector2.zero;
        }
    }
}
