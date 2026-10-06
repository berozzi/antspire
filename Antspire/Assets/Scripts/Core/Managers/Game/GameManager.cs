using UnityEngine;

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

/// Orkiestruje przebieg gry: przetwarza wejście z InputManagera, aktualizuje
/// komponenty w zależności od stanu i reaguje na kliknięcia obiektów.
/// Przejął odpowiedzialność za stan gry (GameStates) po usunięciu GameState.
public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private RaycastManager raycastManager;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private GameplayCanvasManager gameplayCanvasManager;

    public static GameStates CurrentGameState { get; private set; } = GameStates.InGame;

    // Przy wejściu w play mode z wyłączonym domain reloadem statyczny stan gry
    // przeżyłby między sesjami (np. zostawałby BuildMode) - resetujemy go zawsze.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CurrentGameState = GameStates.InGame;
    }

    // Eventy dla UI
    public event System.Action<bool> OnTechTreeToggled;
    public event System.Action<bool> OnWarPanelToggled;
    public event System.Action<bool> OnStructureMenuToggled;
    public event System.Action<GameStates, GameStates> OnStateChanged;

    private void Awake()
    {
        FindMissingReferences();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void FindMissingReferences()
    {
        if (inputManager == null)
            inputManager = FindAnyObjectByType<InputManager>();

        if (buildingManager == null)
            buildingManager = FindAnyObjectByType<BuildingManager>();

        if (raycastManager == null)
            raycastManager = FindAnyObjectByType<RaycastManager>();

        if (hudManager == null)
            hudManager = FindAnyObjectByType<HUDManager>();

        if (gameplayCanvasManager == null)
            gameplayCanvasManager = FindAnyObjectByType<GameplayCanvasManager>();
    }

    private void SubscribeToEvents()
    {
        if (inputManager != null)
        {
            inputManager.OnBuildModeKeyPressed += ToggleBuildMode;
            inputManager.OnDestroyModeKeyPressed += ToggleDestroyMode;
            inputManager.OnEscapePressed += HandleEscapeKey;
            inputManager.OnPrimaryClickPressed += HandlePrimaryClick;
        }

        if (raycastManager != null)
        {
            raycastManager.OnHoverEnter += HandleHoverEnter;
            raycastManager.OnHoverExit += HandleHoverExit;
            raycastManager.OnClickableClicked += HandleClickableClick;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (inputManager != null)
        {
            inputManager.OnBuildModeKeyPressed -= ToggleBuildMode;
            inputManager.OnDestroyModeKeyPressed -= ToggleDestroyMode;
            inputManager.OnEscapePressed -= HandleEscapeKey;
            inputManager.OnPrimaryClickPressed -= HandlePrimaryClick;
        }

        if (raycastManager != null)
        {
            raycastManager.OnHoverEnter -= HandleHoverEnter;
            raycastManager.OnHoverExit -= HandleHoverExit;
            raycastManager.OnClickableClicked -= HandleClickableClick;
        }
    }

    private void Update()
    {
        if (raycastManager != null)
            raycastManager.HandleMainRaycast();

        UpdateComponentsStates();
    }

    private void ToggleBuildMode()
    {
        TogglePanel(GameStates.BuildMode);
    }

    private void ToggleDestroyMode()
    {
        TogglePanel(GameStates.DestroyMode);
    }

    public void TogglePanel(GameStates state)
    {
        SetGameState(CurrentGameState == state ? GameStates.InGame : state);
    }

    public void SetGameState(GameStates newState)
    {
        if (CurrentGameState == newState) return;

        GameStates previousState = CurrentGameState;
        CurrentGameState = newState;

        HandlePanelToggleEvents(previousState, newState);
        OnStateChanged?.Invoke(previousState, newState);
        HandleStateChanged(previousState, newState);

        Debug.Log($"GameState changed: {previousState} -> {newState}");
    }

    private void HandlePanelToggleEvents(GameStates previousState, GameStates newState)
    {
        if (previousState == GameStates.TechTree && newState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(false);
        else if (newState == GameStates.TechTree && previousState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(true);

        if (previousState == GameStates.WarPanel && newState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(false);
        else if (newState == GameStates.WarPanel && previousState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(true);

        if (previousState == GameStates.InStructureMenu && newState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(false);
        else if (newState == GameStates.InStructureMenu && previousState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(true);
    }

    private void HandleEscapeKey()
    {
        switch (CurrentGameState)
        {
            case GameStates.InGame:
                if (hudManager != null)
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

    private void HandlePrimaryClick()
    {
        // W trybie budowy/niszczenia klik obsługuje BuildingManager.
        if (CurrentGameState == GameStates.BuildMode ||
            CurrentGameState == GameStates.DestroyMode)
            return;

        if (raycastManager != null)
            raycastManager.HandleRaycastInput();
    }

    private void UpdateComponentsStates()
    {
        bool shouldBeActive = hudManager == null || !hudManager.isPaused;

        if (buildingManager != null)
        {
            buildingManager.SetBuildModeActive(CurrentGameState == GameStates.BuildMode && shouldBeActive);
            buildingManager.SetDestroyModeActive(CurrentGameState == GameStates.DestroyMode && shouldBeActive);
        }
    }

    /// Podpina prefab struktury, a potem przekazuje go do BuildingManagera.
    public void SetPrefab(GameObject prefab, int width, int height)
    {
        if (buildingManager != null)
            buildingManager.SetStructurePrefab(prefab, width, height);
    }

    private void HandleStateChanged(GameStates previousState, GameStates newState)
    {
        if (gameplayCanvasManager == null)
            return;

        if (newState == GameStates.QueenShop)
            gameplayCanvasManager.TogglePheromoneShop(true);
        else if (previousState == GameStates.QueenShop)
            gameplayCanvasManager.TogglePheromoneShop(false);
    }
    // obsługa kliknięcia na obiektach implementujących IClickable
    private void HandleClickableClick(IClickable clickable, ClickableData data)
    {
        clickable.OnClick();
        Debug.Log($"Clicked on: {data.Name} of type {data.Type}");

        if (clickable is QueenPheromoneShop)
        {
            SetGameState(GameStates.QueenShop);
        }
        else if (hudManager != null)
        {
            hudManager.ShowObjectInfo(data);
        }
    }

    private void HandleHoverEnter(GameObject obj)
    {
        if (obj.TryGetComponent(out HoverHighlight highlight))
            highlight.EnableHighlight();
    }

    private void HandleHoverExit(GameObject obj)
    {
        if (obj.TryGetComponent(out HoverHighlight highlight))
            highlight.DisableHighlight();
    }
}