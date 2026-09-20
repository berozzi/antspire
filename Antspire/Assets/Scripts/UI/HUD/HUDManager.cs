using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject menuPanel;
    public GameObject menuButton;

    [Header("Info Panel References (More in comments in code)")]
    public GameObject objectInfoPanel; // this is a parent panel for infoPanel
    public GameObject infoPanel; // child of objectInfoPanel

    [Header("Info Panel Script Reference")]
    [SerializeField] InfoPanel infoPanelScript;

    [Header("Inventory References")]
    public GameObject inventoryPanel;

    public bool isPaused = false;
    bool isInventoryOpen = false;

    void Start()
    {
        CloseAllPanels();
        menuButton.SetActive(true);
        inventoryPanel.SetActive(true);
        if (infoPanelScript == null)
        {
            infoPanelScript = infoPanel.GetComponent<InfoPanel>();
        }
    }

    // Menu Methods
    public void ToggleMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        menuPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        isPaused = true;
        menuPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void LeaveGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Object Info Methods
    public void ShowObjectInfo(ClickableData data)
    {
        objectInfoPanel.SetActive(true);
        infoPanelScript.ShowInfoWithData(data);
    }

    public void HideObjectInfo()
    {
        objectInfoPanel.SetActive(false);
    }

    // Inventory Methods
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
    }

    private void CloseAllPanels()
    {
        menuPanel.SetActive(false);
        objectInfoPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }
}