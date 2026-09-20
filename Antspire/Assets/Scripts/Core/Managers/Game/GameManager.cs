using UnityEngine;

/// Orkiestruje przebieg gry: przetwarza wejście z InputManagera, aktualizuje
/// komponenty w zależności od stanu i reaguje na kliknięcia obiektów.
public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameState gameState;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private RaycastManager raycastManager;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private GameplayCanvasManager gameplayCanvasManager;

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
        if (gameState == null)
            gameState = FindAnyObjectByType<GameState>();

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

        if (gameState != null)
            gameState.OnStateChanged += HandleStateChanged;
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

        if (gameState != null)
            gameState.OnStateChanged -= HandleStateChanged;
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
        if (gameState != null)
            gameState.TogglePanel(state);
    }

    private void HandleEscapeKey()
    {
        switch (GameState.CurrentGameState)
        {
            case GameStates.InGame:
                hudManager.ToggleMenu();
                break;
            case GameStates.BuildMode:
                gameState.SetGameState(GameStates.InStructureMenu);
                break;
            case GameStates.InStructureMenu:
            case GameStates.DestroyMode:
            case GameStates.QueenShop:
            case GameStates.TechTree:
            case GameStates.WarPanel:
                gameState.SetGameState(GameStates.InGame);
                break;
        }
    }

    private void HandlePrimaryClick()
    {
        // W trybie budowy/niszczenia klik obsługuje BuildingManager.
        if (GameState.CurrentGameState == GameStates.BuildMode ||
            GameState.CurrentGameState == GameStates.DestroyMode)
            return;

        if (raycastManager != null)
            raycastManager.HandleRaycastInput();
    }

    private void UpdateComponentsStates()
    {
        bool shouldBeActive = !hudManager.isPaused;

        if (buildingManager != null)
        {
            buildingManager.SetBuildModeActive(GameState.CurrentGameState == GameStates.BuildMode && shouldBeActive);
            buildingManager.SetDestroyModeActive(GameState.CurrentGameState == GameStates.DestroyMode && shouldBeActive);
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
            gameState.SetGameState(GameStates.QueenShop);
        }
        else
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