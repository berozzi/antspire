using System.IO;
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


    public bool isPaused = false;
    bool isInventoryOpen = false;

    string customPath = "C:\\Users\\GracjanCode\\Desktop\\Ants\\Saves\\gameSave.json";

    void Start()
    {
        // Upewnij siê, ¿e HUD zaczyna w odpowiednim stanie
        CloseAllPanels();
        menuButton.SetActive(true);
        inventoryPanel.SetActive(true);
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
    public void SaveGame()
    {
        // Tutaj dodaj logikê zapisywania gry
        GameSave gameSave = GameSave.Instance;
       
        if (gameSave != null) 
        {
            gameSave.player = new PlayerData()
            {
                coins = 70000,
                feromones = 5678,
                wisdomPoints = 1234
            };
            //gameSave.resources.Add(new ResourceData()
            //{
            //    id = System.Guid.NewGuid().ToString(),
            //    resourceName = "Wood",
            //    type = ResourceType.Wood,
            //    x = 10,
            //    y = 20,
            //    value = 100,
            //    isAvailable = true
            //});

            SaveGameToFile(gameSave);
        }
    }
    private void SaveGameToFile(GameSave gameSave)
    {
        string json = JsonUtility.ToJson(gameSave, true);
        File.WriteAllText(customPath, json);
        Debug.Log("Game Saved with New Input System!");
    }

    public void LoadGame()
    {
        // Tutaj dodaj logikê ³adowania gry
        if (File.Exists(customPath))
        {
            string json = File.ReadAllText(customPath);
            GameSave loadedGameSave = JsonUtility.FromJson<GameSave>(json);
            GameSave.Instance.player = loadedGameSave.player;
            GameSave.Instance.resources = loadedGameSave.resources;
            Debug.Log("Game Loaded with New Input System!");
        }
        else
        {
            Debug.LogWarning("No save file found at: " + customPath);
        }
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

    private void CloseAllPanels()
    {
        menuPanel.SetActive(false);
        objectInfoPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }
}