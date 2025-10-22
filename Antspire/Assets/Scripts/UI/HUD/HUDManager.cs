using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject menuPanel;
    public GameObject menuButton;

    [Header("Info Panel References")]
    public GameObject objectInfoPanel;

    [Header("Inventory References")]
    public GameObject inventoryPanel;

    [Header("Minimap References")]
    public GameObject minimap;

    public bool isPaused = false;
    bool isInventoryOpen = false;


    void Start()
    {
        // Upewnij siê, ¿e HUD zaczyna w odpowiednim stanie
        CloseAllPanels();
        menuButton.SetActive(true);
        minimap.SetActive(true);
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
        minimap.SetActive(true);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        isPaused = true;
        menuPanel.SetActive(true);
        minimap.SetActive(false);
        Time.timeScale = 0f;
    }
    public void SaveGame()
    {
        // Tutaj dodaj logikê zapisywania gry
        Debug.Log("Gra zapisana!");
    }

    public void LeaveGame()
    {
        Time.timeScale = 1f; // Upewnij siê, ¿e czas p³ynie normalnie
        SceneManager.LoadScene("MainMenu"); // Zamieñ na nazwê swojej sceny menu
    }

    // Object Info Methods
    public void ShowObjectInfo(string objectName, string description)
    {
        objectInfoPanel.SetActive(true);
        // Tutaj mo¿esz ustawiæ tekst w panelu informacji
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

    public void ShowInventory()
    {
        isInventoryOpen = true;
        inventoryPanel.SetActive(true);
    }

    public void HideInventory()
    {
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);
    }

    private void CloseAllPanels()
    {
        menuPanel.SetActive(false);
        objectInfoPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }
}