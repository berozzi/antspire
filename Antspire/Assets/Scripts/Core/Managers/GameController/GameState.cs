using UnityEngine;
public enum GameStates
{
    InGame,
    Paused,
    BuildMode,
    DestroyMode,
    TechTree,
    WarPanel,
    InStructureMenu
}
public class GameState : MonoBehaviour
{
    public static GameStates CurrentGameState { get; private set; } = GameStates.InGame;
    [SerializeField] PlaceDownStructure placeDownStructure;
    [SerializeField] DeleteStructure deleteStructure;
    [SerializeField] HUDManager hudManager;
    [SerializeField] GameplayCanvasManager gameplayCanvasManager;

    [Header("Test Structure Selection")]
    [SerializeField] GameObject structurePrefab;
    
    [Header("Input Settings")]
    [SerializeField] KeyCode buildModeKey = KeyCode.B;
    [SerializeField] KeyCode destroyModeKey = KeyCode.N;
    [SerializeField] KeyCode exitModesKey = KeyCode.Escape;

    // Eventy dla UI
    public event System.Action<bool> OnTechTreeToggled;
    public event System.Action<bool> OnWarPanelToggled;
    public event System.Action<bool> OnStructureMenuToggled;
    void Awake()
    {
        FindMissingReferences();
    }

    void Start() {}

    void Update()
    {
        UpdateComponentsStates();
        HandleInput();
    }

    private void FindMissingReferences()
    {
        if (placeDownStructure == null)
            placeDownStructure = FindAnyObjectByType<PlaceDownStructure>();
        if (deleteStructure == null)
            deleteStructure = FindAnyObjectByType<DeleteStructure>();
        if (hudManager == null)
            hudManager = FindAnyObjectByType<HUDManager>();
        if (gameplayCanvasManager == null)
            gameplayCanvasManager = FindAnyObjectByType<GameplayCanvasManager>();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(buildModeKey))
            TogglePanel(GameStates.BuildMode);

        if (Input.GetKeyDown(destroyModeKey))
            TogglePanel(GameStates.DestroyMode);

        if (Input.GetKeyDown(exitModesKey))
            HandleEscapeKey();
    }

    public void TogglePanel(GameStates current)
    {     
        SetGameState(CurrentGameState == current ? GameStates.InGame : current);
    }

    private void HandleEscapeKey()
    {
        switch (CurrentGameState)
        {
            case GameStates.InGame:
                hudManager.ToggleMenu();
                break;
            case GameStates.BuildMode:
                SetGameState(GameStates.InStructureMenu);
                break;
            case GameStates.InStructureMenu:
            case GameStates.DestroyMode:
            case GameStates.TechTree:
            case GameStates.WarPanel:
                SetGameState(GameStates.InGame);
                break;
        }
    }

    private void SetGameState(GameStates newState)
    {
        if (CurrentGameState == newState) return;

        GameStates previousState = CurrentGameState;
        CurrentGameState = newState;

        // Wywo³aj eventy przy zmianie stanu
        HandleStateChangeEvents(previousState, newState);

        // Mo¿esz te¿ dodaæ aktualizacjê UI
        //if (gameplayCanvasManager != null)
        //    gameplayCanvasManager.OnGameStateChanged(previousState, newState);

        Debug.Log($"GameState changed: {previousState} -> {newState}");
    }

    private void HandleStateChangeEvents(GameStates previousState, GameStates newState)
    {
        // TechTree events
        if (previousState == GameStates.TechTree && newState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(false);
        else if (newState == GameStates.TechTree && previousState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(true);

        // WarPanel events
        if (previousState == GameStates.WarPanel && newState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(false);
        else if (newState == GameStates.WarPanel && previousState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(true);

        // StructureMenu events
        if (previousState == GameStates.InStructureMenu && newState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(false);
        else if (newState == GameStates.InStructureMenu && previousState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(true);
    }

    private void UpdateComponentsStates()
    {
        bool shouldBeActive = !hudManager.isPaused;

        if (placeDownStructure != null)
            placeDownStructure.enabled = CurrentGameState == GameStates.BuildMode && shouldBeActive;

        if (deleteStructure != null)
            deleteStructure.enabled = CurrentGameState == GameStates.DestroyMode && shouldBeActive;
    }

    public void SetPrefab(GameObject prefab)
    {
        structurePrefab = prefab;
        if (placeDownStructure != null && structurePrefab != null)
        {
            placeDownStructure.SetStructurePrefab(structurePrefab);
        }
    }
}