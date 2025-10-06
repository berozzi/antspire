using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [Header("Inventory Panel")]
    public GameObject inventoryPanel;
    public Transform inventorySlotsParent;

    [Header("Object Info Panel")]
    public GameObject objectInfoPanel;
    public Text objectNameText;
    public Text objectDescriptionText;
    public Image objectIconImage;

    [Header("Menu Panel")]
    public GameObject menuPanel;
    public GameObject unsavedProgressWarning;
    public Button resumeButton;
    public Button saveButton;
    public Button loadButton;
    public Button exitButton;

    [Header("Minimap")]
    public Camera minimapCamera;
    public RawImage minimapRender;
    public RenderTexture minimapTexture;

    [Header("Settings")]
    public KeyCode inventoryKey = KeyCode.I;
    public KeyCode menuKey = KeyCode.Escape;
    public float minimapDistance = 50f;
    public Vector3 minimapOffset = new Vector3(0, 50, 0);

    private bool isGamePaused = false;
    private bool hasUnsavedProgress = false;
    private GameObject currentSelectedObject;

    void Start()
    {
        InitializeHUD();
        SetupMinimapCamera();
    }

    void Update()
    {
        HandleInput();
    
        UpdateMinimapIndicator();
    }

    void InitializeHUD()
    {
        // Ustawienie pocz¹tkowego stanu paneli
        inventoryPanel.SetActive(false);
        objectInfoPanel.SetActive(false);
        menuPanel.SetActive(false);
        unsavedProgressWarning.SetActive(false);

        // Podpiêcie eventów do przycisków menu
        resumeButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        exitButton.onClick.AddListener(ShowExitWarning);

        // Dodanie listenerów do przycisków warninga
        Button[] warningButtons = unsavedProgressWarning.GetComponentsInChildren<Button>();
        warningButtons[0].onClick.AddListener(ExitWithoutSaving); // Tak - wyjdŸ
        warningButtons[1].onClick.AddListener(HideExitWarning);   // Nie - wróæ
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera != null)
        {
            // Utworzenie tekstury dla minimapy
            minimapTexture = new RenderTexture(256, 256, 16);
            minimapCamera.targetTexture = minimapTexture;
            minimapRender.texture = minimapTexture;

            // Konfiguracja kamery minimapy
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 20f;
            minimapCamera.transform.position = minimapOffset;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            minimapCamera.gameObject.SetActive(true);
        }
    }

    void HandleInput()
    {
        // Otwieranie/zamykanie inventory
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }

        // Otwieranie/zamykanie menu
        if (Input.GetKeyDown(menuKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeInHierarchy;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateInventoryUI();
        }
    }

    //public void ShowObjectInfo(GameObject targetObject)
    //{
    //    currentSelectedObject = targetObject;
    //    //ObjectInfo objectInfo = targetObject.GetComponent<ObjectInfo>();

    //    if (objectInfo != null)
    //    {
    //        objectNameText.text = objectInfo.objectName;
    //        objectDescriptionText.text = objectInfo.description;
    //        objectIconImage.sprite = objectInfo.icon;
    //        objectInfoPanel.SetActive(true);
    //    }
    //}

    //public void HideObjectInfo()
    //{
    //    objectInfoPanel.SetActive(false);
    //    currentSelectedObject = null;
    //}

    void ToggleMenu()
    {
        isGamePaused = !menuPanel.activeInHierarchy;
        menuPanel.SetActive(isGamePaused);

        Time.timeScale = isGamePaused ? 0f : 1f;
        Cursor.lockState = isGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isGamePaused;
    }

    public void ResumeGame()
    {
        menuPanel.SetActive(false);
        isGamePaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveGame()
    {
        // Tutaj implementacja zapisywania gry
        Debug.Log("Gra zapisana!");
        hasUnsavedProgress = false;

        // Przyk³adowa implementacja zapisu
        // PlayerPrefs.SetString("SavedGame", JsonUtility.ToJson(gameData));
        // PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        // Tutaj implementacja wczytywania gry
        Debug.Log("Gra wczytana!");

        // Przyk³adowa implementacja wczytywania
        // if (PlayerPrefs.HasKey("SavedGame"))
        // {
        //     gameData = JsonUtility.FromJson<GameData>(PlayerPrefs.GetString("SavedGame"));
        // }
    }

    public void ShowExitWarning()
    {
        if (hasUnsavedProgress)
        {
            unsavedProgressWarning.SetActive(true);
        }
        else
        {
            ExitGame();
        }
    }

    public void HideExitWarning()
    {
        unsavedProgressWarning.SetActive(false);
    }

    public void ExitWithoutSaving()
    {
        unsavedProgressWarning.SetActive(false);
        ExitGame();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void UpdateInventoryUI()
    {
        // Tutaj aktualizacja UI inventory
        // Przyk³ad: 
        // foreach (Transform slot in inventorySlotsParent)
        // {
        //     // Aktualizuj ka¿dy slot
        // }
    }

    // Metoda do oznaczania niezapisanego postêpu
    public void MarkUnsavedProgress()
    {
        hasUnsavedProgress = true;
    }

    // Aktualizacja pozycji kamery minimapy (wywo³ywana np. gdy gracz siê porusza)
    public void UpdateMinimapPosition(Vector3 playerPosition)
    {
        if (minimapCamera != null)
        {
            Vector3 newPos = playerPosition + minimapOffset;
            minimapCamera.transform.position = newPos;
        }
    }
    [Header("Minimap Indicator")]
    public RectTransform playerIndicator;
    public Transform playerTransform;

    void UpdateMiniMapIndicator()
    {
        // ... Twój istniej¹cy kod ...

        UpdateMinimapIndicator();
    }

    // KROK 8B: WskaŸnik kierunku gracza
    void UpdateMinimapIndicator()
    {
        if (playerIndicator != null && playerTransform != null)
        {
            // Obracaj wskaŸnik zgodnie z rotacj¹ gracza
            playerIndicator.rotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
        }
    }

    // KROK 8C: Zmiana widoku minimapy
    public void ToggleMinimapView()
    {
        if (minimapCamera.orthographic)
        {
            // Tryb perspektywy
            minimapCamera.orthographic = false;
            minimapCamera.fieldOfView = 60f;
        }
        else
        {
            // Tryb ortograficzny
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 20f;
        }
    }

    // KROK 8D: Zmiana zoomu minimapy
    public void SetMinimapZoom(float newSize)
    {
        if (minimapCamera != null && minimapCamera.orthographic)
        {
            minimapCamera.orthographicSize = Mathf.Clamp(newSize, 5f, 50f);
        }
    }
}

// Klasa pomocnicza dla informacji o obiektach
//[System.Serializable]
//public class ObjectInfo : MonoBehaviour
//{
//    public string objectName;
//    [TextArea(3, 10)]
//    public string description;
//    public Sprite icon;
//}