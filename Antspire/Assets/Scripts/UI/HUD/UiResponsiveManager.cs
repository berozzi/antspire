using UnityEngine;
using UnityEngine.UI;

public class UIResponsiveManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject infoPanel;

    [Header("Buttons")]
    public Button menuButton;
    public Button menuCancelButton;
    public Button infoCancelButton;

    [Header("Responsive Settings")]
    public bool enableResponsive = true;
    public float minScreenWidth = 800f;
    public float minScreenHeight = 600f;

    private Canvas canvas;
    private CanvasScaler canvasScaler;

    void Start()
    {
        // Pobierz komponenty Canvas
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        // Setup button listeners
        SetupButtonListeners();

        // Initial responsive setup
        UpdateUIResponsiveness();
    }

    void Update()
    {
        // Update responsiveness if screen size changes
        if (enableResponsive)
        {
            UpdateUIResponsiveness();
        }

        // Handle Escape key for closing panels
        HandleEscapeKey();
    }

    void SetupButtonListeners()
    {
        // Menu Button - opens Menu Panel
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OpenMenuPanel);
        }

        // Menu Cancel Button - closes Menu Panel
        if (menuCancelButton != null)
        {
            menuCancelButton.onClick.RemoveAllListeners();
            menuCancelButton.onClick.AddListener(CloseMenuPanel);
        }

        // Info Cancel Button - closes Info Panel
        if (infoCancelButton != null)
        {
            infoCancelButton.onClick.RemoveAllListeners();
            infoCancelButton.onClick.AddListener(CloseInfoPanel);
        }

        Debug.Log("Button listeners setup completed");
    }

    void HandleEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (infoPanel != null && infoPanel.activeInHierarchy)
            {
                CloseInfoPanel();
            }
            else if (menuPanel != null && menuPanel.activeInHierarchy)
            {
                CloseMenuPanel();
            }
            else
            {
                OpenMenuPanel();
            }
        }
    }

    // Public methods for button events
    public void OpenMenuPanel()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            Debug.Log("Menu Panel opened");

            // Pause game if needed
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("Menu Panel reference is missing!");
        }
    }

    public void CloseMenuPanel()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            Debug.Log("Menu Panel closed");

            // Resume game
            Time.timeScale = 1f;
        }
    }

    public void CloseInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
            Debug.Log("Info Panel closed");
        }
    }

    void UpdateUIResponsiveness()
    {
        if (!enableResponsive || canvasScaler == null) return;

        float currentWidth = Screen.width;
        float currentHeight = Screen.height;

        // Adjust scale based on screen size
        float scaleFactor = CalculateScaleFactor(currentWidth, currentHeight);
        canvasScaler.scaleFactor = scaleFactor;

        // Adjust button sizes for touch devices
        AdjustButtonSizes(scaleFactor);

        // Adjust panel positions for different aspect ratios
        AdjustPanelPositions();
    }

    float CalculateScaleFactor(float width, float height)
    {
        float referenceWidth = canvasScaler.referenceResolution.x;
        float referenceHeight = canvasScaler.referenceResolution.y;

        // Calculate scale based on width and height
        float widthRatio = width / referenceWidth;
        float heightRatio = height / referenceHeight;

        // Use the smaller ratio to ensure everything fits
        float scale = Mathf.Min(widthRatio, heightRatio);

        // Clamp scale to reasonable values
        return Mathf.Clamp(scale, 0.5f, 2f);
    }

    void AdjustButtonSizes(float scaleFactor)
    {
        // U¯YJ FindObjectsByType ZAMIAST PRZESTARZA£EGO FindObjectsOfType
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in allButtons)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Adjust minimum size for touch devices
                if (IsTouchDevice())
                {
                    float minSize = 100f * scaleFactor;
                    if (rectTransform.sizeDelta.x < minSize)
                    {
                        rectTransform.sizeDelta = new Vector2(minSize, rectTransform.sizeDelta.y);
                    }
                    if (rectTransform.sizeDelta.y < minSize)
                    {
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, minSize);
                    }
                }
            }
        }
    }

    void AdjustPanelPositions()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        // Adjust for ultra-wide screens
        if (aspectRatio > 2.0f)
        {
            // Move panels closer to center on ultra-wide screens
            AdjustPanelForUltraWide();
        }
        // Adjust for narrow screens
        else if (aspectRatio < 1.3f)
        {
            // Move panels closer to center on narrow screens
            AdjustPanelForNarrow();
        }
        else
        {
            // Default positions
            ResetPanelPositions();
        }
    }

    void AdjustPanelForUltraWide()
    {
        // Example: Move side panels closer to center
        if (menuPanel != null)
        {
            RectTransform rect = menuPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.25f, rect.anchorMin.y);
                rect.anchorMax = new Vector2(0.75f, rect.anchorMax.y);
            }
        }
    }

    void AdjustPanelForNarrow()
    {
        // Example: Adjust for narrow screens
        if (infoPanel != null)
        {
            RectTransform rect = infoPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.1f, rect.anchorMin.y);
                rect.anchorMax = new Vector2(0.9f, rect.anchorMax.y);
            }
        }
    }

    void ResetPanelPositions()
    {
        // Reset to default positions
        if (menuPanel != null)
        {
            RectTransform rect = menuPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.25f, 0.25f);
                rect.anchorMax = new Vector2(0.75f, 0.75f);
            }
        }

        if (infoPanel != null)
        {
            RectTransform rect = infoPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.7f, 0.3f);
                rect.anchorMax = new Vector2(0.95f, 0.7f);
            }
        }
    }

    bool IsTouchDevice()
    {
        return Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer;
    }

    // Public method to toggle responsive mode
    public void ToggleResponsiveMode(bool enabled)
    {
        enableResponsive = enabled;
        UpdateUIResponsiveness();
    }

    // Debug info
    void OnRectTransformDimensionsChange()
    {
        if (enableResponsive)
        {
            Debug.Log($"Screen size changed: {Screen.width}x{Screen.height}");
            UpdateUIResponsiveness();
        }
    }

    // Additional utility method using FindObjectsByType
    Button[] FindAllButtonsInScene()
    {
        return FindObjectsByType<Button>(FindObjectsSortMode.None);
    }

    // Method to find specific button by name
    Button FindButtonByName(string buttonName)
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in allButtons)
        {
            if (button.name == buttonName)
            {
                return button;
            }
        }

        return null;
    }
}