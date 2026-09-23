using UnityEngine;
using UnityEngine.InputSystem;

/// Zarządza stawianiem i usuwaniem budynków na gridzie.
/// Wchłonął logikę PlaceDownStructure, DeleteStructure oraz selekcję budynku.
public class BuildingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager grid;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private NavMeshManager navMeshManager;
    [SerializeField] private InputManager inputManager;

    [Header("Highlight Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    private GameObject structurePrefab;
    private bool buildModeEnabled;
    private bool destroyModeEnabled;

    private GameObject currentHighlight;
    private MeshRenderer highlightRenderer;
    private Vector2Int lastGridPos;
    private bool wasValidLastFrame = true;

    private int prefabWidth = 1;
    private int prefabHeight = 1;
    private LayerMask groundLayerMask;

    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();

        if (hudManager == null)
            hudManager = FindAnyObjectByType<HUDManager>();

        if (navMeshManager == null)
            navMeshManager = FindAnyObjectByType<NavMeshManager>();

        if (inputManager == null)
            inputManager = FindAnyObjectByType<InputManager>();

        groundLayerMask = LayerMask.GetMask("Ground");

        if (inputManager != null)
            inputManager.OnPrimaryClickPressed += HandlePrimaryClick;
    }

    private void OnDestroy()
    {
        if (inputManager != null)
            inputManager.OnPrimaryClickPressed -= HandlePrimaryClick;

        if (currentHighlight != null)
            Destroy(currentHighlight);
    }

    /// Ustawia prefab budynku i jego rozmiar (w komórkach).
    public void SetStructurePrefab(GameObject prefab, int width, int height)
    {
        structurePrefab = prefab;
        prefabWidth = width;
        prefabHeight = height;
    }

    /// Zarządza aktywnością trybu budowy (świetlnie highlight'u).
    public void SetBuildModeActive(bool active)
    {
        buildModeEnabled = active;

        if (currentHighlight != null)
            currentHighlight.SetActive(active);
    }

    public void SetDestroyModeActive(bool active)
    {
        destroyModeEnabled = active;
    }

    private void Update()
    {
        if (buildModeEnabled && structurePrefab != null)
            UpdateHighlight();
    }

    private void HandlePrimaryClick()
    {
        if (hudManager != null && hudManager.isPaused)
            return;

        if (buildModeEnabled)
            PlaceBuilding();
        else if (destroyModeEnabled)
            DeleteBuilding();
    }

    private void PlaceBuilding()
    {
        var (success, mousePos) = GetMouseWorldPosition();
        if (!success)
        {
            Debug.Log("Kliknięto poza Ground, nie stawiam budynku.");
            return;
        }

        Vector3 gridPos = SnapBuildingToGrid(mousePos);
        Vector2Int gridCoord = grid != null ? grid.WorldToGrid(gridPos) : Vector2Int.zero;

        if (!CanPlaceBuilding(gridCoord))
        {
            Debug.Log("Cannot place structure here, area is occupied. " + gridPos);
            return;
        }

        Instantiate(structurePrefab, gridPos, Quaternion.identity);
        Debug.Log($"Placed structure at grid position: {gridCoord}");
        OccupyCells(gridCoord, true);

        // ta metoda musi być asynchroniczna żeby nie powodować przycięcia klatki przy dużej ilości obiektów
        if (navMeshManager != null)
            navMeshManager.RebuildNavMesh();
    }

    private bool CanPlaceBuilding(Vector2Int baseCoord)
    {
        if (grid == null || structurePrefab == null)
            return false;

        for (int x = 0; x < prefabWidth; x++)
        {
            for (int z = 0; z < prefabHeight; z++)
            {
                Vector2Int checkCoord = baseCoord + new Vector2Int(x, z);

                Cell cell = grid.GetCell(checkCoord);
                if (cell == null)
                    return false;

                if (cell.isOccupied)
                    return false;
            }
        }

        return true;
    }

    /// Zmienia stan zajętości komórek pod budynkiem.
    public void OccupyCells(Vector2Int baseCoord, bool state)
    {
        if (grid == null)
            return;

        for (int x = 0; x < prefabWidth; x++)
        {
            for (int z = 0; z < prefabHeight; z++)
            {
                Vector2Int coord = baseCoord + new Vector2Int(x, z);
                Cell cell = grid.GetCell(coord);

                if (cell != null)
                    cell.isOccupied = state;
            }
        }
    }

    /// Pobiera pozycję świata pod kursorem (raycast na warstwę Ground).
    public (bool success, Vector3 point) GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Camera.main is null!");
            return (false, Vector3.zero);
        }

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            return (true, hit.point);

        return (false, Vector3.zero);
    }

    /// Przyciąga pozycję świata do siatki.
    public Vector3 SnapBuildingToGrid(Vector3 worldPos)
    {
        if (grid == null)
            return worldPos;

        Vector2Int gridCoord = grid.WorldToGrid(worldPos);
        return grid.GridToWorld(gridCoord);
    }

    private void DeleteBuilding()
    {
        var (success, _) = GetMouseWorldPosition();

        if (!success)
        {
            Debug.Log("Kliknięto poza Ground – nie usuwam.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Camera.main is null!");
            return;
        }

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = cam.ScreenPointToRay(mousePos);

        int layerMask = ~LayerMask.GetMask("Ground");
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Nie trafiono żadnego obiektu z koliderem do usunięcia.");
            return;
        }

        GameObject hitObject = hit.collider.gameObject;

        if (hitObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Kliknięto Ground – brak obiektu do usunięcia.");
            return;
        }

        Vector3 buildingWorldPos = hitObject.transform.position;
        Vector2Int gridCoord = grid != null ? grid.WorldToGrid(buildingWorldPos) : Vector2Int.zero;

        OccupyCells(gridCoord, false);
        Destroy(hitObject);
        Debug.Log($"Usunięto obiekt z koliderem na pozycji siatki: {gridCoord}");

        if (navMeshManager != null)
            navMeshManager.RebuildNavMesh();
    }

    // ---------- Highlight pod budową ----------

    private void UpdateHighlight()
    {
        var (success, mousePosHighlight) = GetMouseWorldPosition();
        if (!success)
            return;

        Vector3 gridPosHighlight = SnapBuildingToGrid(mousePosHighlight);
        Vector2Int gridCoordHighlight = grid != null ? grid.WorldToGrid(gridPosHighlight) : Vector2Int.zero;

        if (gridCoordHighlight != lastGridPos || currentHighlight == null)
        {
            CreateOrUpdateHighlight(gridPosHighlight);
            lastGridPos = gridCoordHighlight;
        }

        bool isValid = CanPlaceBuilding(gridCoordHighlight);
        UpdateHighlightColor(isValid);
    }

    private void CreateOrUpdateHighlight(Vector3 worldPos)
    {
        if (currentHighlight == null)
            CreateHighlightObject();

        currentHighlight.transform.position = worldPos;
        currentHighlight.transform.localScale = new Vector3(prefabWidth, 0.1f, prefabHeight * 0.9f);
    }

    private void CreateHighlightObject()
    {
        currentHighlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentHighlight.name = "BuildingHighlight";

        Destroy(currentHighlight.GetComponent<Collider>());

        highlightRenderer = currentHighlight.GetComponent<MeshRenderer>();
        highlightRenderer.material = highlightMaterial;
    }

    private void UpdateHighlightColor(bool isValid)
    {
        if (highlightRenderer == null)
            return;

        if (isValid != wasValidLastFrame)
        {
            highlightRenderer.material = isValid ? highlightMaterial : invalidMaterial;
            wasValidLastFrame = isValid;
        }
    }
}