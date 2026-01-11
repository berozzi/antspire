using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameStates
{
    InGame,
    Paused,
    BuildMode,
    DestroyMode,
    TechTree,
    WarPanel,
    InStructureMenu,
    QueenShop
}
public class GameState : MonoBehaviour
{
    public static GameStates CurrentGameState { get; private set; } = GameStates.InGame;
    [SerializeField] PlaceDownStructure placeDownStructure;
    [SerializeField] DeleteStructure deleteStructure;
    [SerializeField] HUDManager hudManager;
    [SerializeField] GameplayCanvasManager gameplayCanvasManager;
    [SerializeField] RaycastManager raycastManager;

    [Header("Test Structure Selection")]
    [SerializeField] GameObject structurePrefab;
    
    [Header("Input Settings")]
    [SerializeField] KeyCode buildModeKey = KeyCode.B;
    [SerializeField] KeyCode destroyModeKey = KeyCode.N;
    [SerializeField] KeyCode exitModesKey = KeyCode.Escape;
    [SerializeField] KeyCode raycastInputKey = KeyCode.Mouse0;

    // Eventy dla UI
    public event System.Action<bool> OnTechTreeToggled;
    public event System.Action<bool> OnWarPanelToggled;
    public event System.Action<bool> OnStructureMenuToggled;

    void Awake()
    {
        FindMissingReferences();
        SubscribeToEvents();
        raycastManager.OnHoverEnter += HandleHoverEnter;
        raycastManager.OnHoverExit += HandleHoverExit;
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
        raycastManager.OnHoverExit -= HandleHoverEnter;
        raycastManager.OnHoverExit -= HandleHoverExit;
    }

    private void SubscribeToEvents()
    {
        if (raycastManager != null)
        {
            raycastManager.OnClickableClicked += HandleClickableClick;
            
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (raycastManager != null)
        {
            raycastManager.OnClickableClicked -= HandleClickableClick;
            
        }
    }

    void Update()
    {
        raycastManager.HandleMainRaycast();
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
        if (raycastManager == null)
        {
            Debug.Log("RaycastManager reference was missing, attempting to find one in the scene.");
            raycastManager = FindAnyObjectByType<RaycastManager>();
        }
            
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(buildModeKey))
            TogglePanel(GameStates.BuildMode);

        if (Input.GetKeyDown(destroyModeKey))
            TogglePanel(GameStates.DestroyMode);

        if (Input.GetKeyDown(exitModesKey))
            HandleEscapeKey();

        if (Input.GetKeyDown(raycastInputKey))
            raycastManager.HandleRaycastInput();
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
            case GameStates.QueenShop:
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

        // QueenShop events
        if (newState == GameStates.QueenShop && gameplayCanvasManager != null)
            gameplayCanvasManager.TogglePheromoneShop(true);
        else if (previousState == GameStates.QueenShop && gameplayCanvasManager != null)
            gameplayCanvasManager.TogglePheromoneShop(false);

    }

    private void UpdateComponentsStates()
    {
        bool shouldBeActive = !hudManager.isPaused;

        if (placeDownStructure != null)
            placeDownStructure.enabled = CurrentGameState == GameStates.BuildMode && shouldBeActive;

        if (deleteStructure != null)
            deleteStructure.enabled = CurrentGameState == GameStates.DestroyMode && shouldBeActive;
    }
    void HandleClickableClick(IClickable clickable, ClickableData data)
    {
        clickable.OnClick();
        Debug.Log($"Clicked on: {data.Name} of type {data.Type}");
        // Identify the type of clickable and perform actions accordingly, for now just two types - queen and structure
        if (clickable is QueenPheromoneShop)
        {
            SetGameState(GameStates.QueenShop);
        }
        else 
        {
            hudManager.ShowObjectInfo(clickable, data);
        }
    }
    // podpina prefaba struktury i tunelu a potem przekazuje to do PlaceDownStructure
    public void SetPrefab(GameObject prefab, int width, int height)
    {
        structurePrefab = prefab;
        if (placeDownStructure != null && structurePrefab != null)
        {
            placeDownStructure.SetStructurePrefab(structurePrefab, width, height);
        }
    }
    void HandleHoverEnter(GameObject obj)
    {
        if (obj.TryGetComponent(out HoverHighlight highlight))
            highlight.EnableHighlight();
    }

    void HandleHoverExit(GameObject obj)
    {
        if (obj.TryGetComponent(out HoverHighlight highlight))
            highlight.DisableHighlight();
    }
}