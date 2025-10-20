using UnityEngine;
public enum GameStates
{
    InGame,
    Paused,
    Inventory,
    BuildMode,
    DestroyMode
}
public class GameState : MonoBehaviour
{
    public static GameStates CurrentGameState { get; private set; } = GameStates.InGame;
    [SerializeField] PlaceDownStructure placeDownStructure;
    [SerializeField] DeleteStructure deleteStructure;
    [SerializeField] HUDManager hudManager;

    [Header("Test Structure Selection")]
    [SerializeField] GameObject testStructurePrefab;
    // Update is called once per frame
    [Header("Input Settings")]
    [SerializeField] KeyCode buildModeKey = KeyCode.B;
    [SerializeField] KeyCode destroyModeKey = KeyCode.N;
    [SerializeField] KeyCode exitModesKey = KeyCode.Escape;
    void Awake()
    {
        if (placeDownStructure == null)
        {
            Debug.LogError("PlaceDownStructure not assigned! Finding object by this type", this);
            placeDownStructure = FindAnyObjectByType<PlaceDownStructure>();
        }
        if (deleteStructure == null)
        {
            Debug.LogError("DeleteStructure not assigned! Finding object by this type", this);
            deleteStructure = FindAnyObjectByType<DeleteStructure>();
        }
        if (hudManager == null)
        {
            Debug.LogError("HUDManager not assigned! Finding object by this type", this);
            hudManager = FindAnyObjectByType<HUDManager>();
        }
    }
    void Start()
    {
        ApplyTestStructure();
    }
    void Update()
    {
        UpdateStructureStates();
        HandleInput();
    }

    private void HandleInput()
    {
        // Klawisz B - Tryb budowania
        if (Input.GetKeyDown(buildModeKey))
        {
            ToggleBuildMode();
        }

        // Klawisz N - Tryb niszczenia
        if (Input.GetKeyDown(destroyModeKey))
        {
            ToggleDestroyMode();
        }

        // Escape - Wyjœcie z trybów
        if (Input.GetKeyDown(exitModesKey))
        {
            HandleEscapeKey();
        }
    }

    private void ToggleBuildMode()
    {
        if (CurrentGameState == GameStates.BuildMode)
        {
            // Jeœli ju¿ jesteœmy w trybie budowania, wróæ do gry
            SetGameState(GameStates.InGame);
        }
        else
        {
            // Prze³¹cz do trybu budowania
            SetGameState(GameStates.BuildMode);
        }
    }

    private void ToggleDestroyMode()
    {
        if (CurrentGameState == GameStates.DestroyMode)
        {
            // Jeœli ju¿ jesteœmy w trybie niszczenia, wróæ do gry
            SetGameState(GameStates.InGame);
        }
        else
        {
            // Prze³¹cz do trybu niszczenia
            SetGameState(GameStates.DestroyMode);
        }
    }
    private void HandleEscapeKey()
    {
        switch (CurrentGameState)
        {
            case GameStates.InGame:
                // W normalnej grze - otwórz menu
                hudManager.ToggleMenu();
                break;

            case GameStates.BuildMode:
            case GameStates.DestroyMode:
                // W trybach specjalnych - wyjdŸ z trybu
                ExitSpecialModes();
                break;

            case GameStates.Paused:
                // W menu - zamknij menu (lub nic nie rób jeœli HUDManager ju¿ to obs³uguje)
                // Mo¿esz zostawiæ puste lub wywo³aæ CloseMenu()
                break;
        }
    }
    private void ExitSpecialModes()
    {
        // Escape zawsze wraca do normalnego trybu gry
        if (CurrentGameState != GameStates.InGame)
        {
            SetGameState(GameStates.InGame);
        }
    }

    private void SetGameState(GameStates newState)
    {
        GameStates previousState = CurrentGameState;
        CurrentGameState = newState;

        Debug.Log($"GameState changed: {previousState} -> {newState}");

        // Opcjonalnie: wywo³aj eventy jeœli potrzebujesz
        // GameEvents.OnGameStateChanged?.Invoke(newState);
    }
    private void UpdateStructureStates()
    {
        bool shouldBeActive = !hudManager.isPaused; // Sprawdza czy gra nie jest w pauzie

        if (placeDownStructure != null)
            placeDownStructure.enabled = CurrentGameState == GameStates.BuildMode && shouldBeActive;
        
        if (deleteStructure != null)
            deleteStructure.enabled = CurrentGameState == GameStates.DestroyMode && shouldBeActive;

        // te dwa if-y sprawdzaj¹ czy komponenty nie s¹ null, zanim spróbuj¹ ustawiæ ich stan enabled
        // oraz kiedy bêd¹ mia³y nadaæ im enabled to równie¿ sprawdzaj¹ czy gra nie jest w pauzie (shouldBeActive), && - dziêki temu siê to dzieje

    }
    void ApplyTestStructure()
    {
        if (testStructurePrefab != null && placeDownStructure != null)
        {
            placeDownStructure.SetStructurePrefab(testStructurePrefab);
        }
    }
}