using System.Collections.Generic;
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

    /// <summary>
    /// Typy komponentów oznaczających "to jest struktura":
    /// można ją usunąć w trybie niszczenia i zajmuje ona komórki siatki.
    /// Mrówki i inne obiekty tych komponentów nie mają, więc tryb niszczenia
    /// ich nie kasuje.
    /// </summary>
    private static readonly System.Type[] StructureComponentTypes =
    {
        typeof(BuildingFootprint),
        typeof(Building),
        typeof(StructureSize),
        typeof(Tunnel),
        typeof(House),
        typeof(Workplace),
        typeof(ResourceProducer)
    };

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

        // Siatka buduje się w swoim Start(); dzięki eventowi nie zależy to
        // od kolejności Start() między obiektami w scenie.
        if (grid != null)
            grid.GridReady += OccupyExistingStructures;

        if (inputManager != null)
            inputManager.OnPrimaryClickPressed += HandlePrimaryClick;
    }

    private void Start()
    {
        // Sytuacja, w której GridManager został utworzony później niż nasz Awake.
        if (grid != null && grid.IsInitialized)
            OccupyExistingStructures();
    }

    private void OnDestroy()
    {
        if (grid != null)
            grid.GridReady -= OccupyExistingStructures;

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

        Vector3 baseCellCenter = SnapBuildingToGrid(mousePos);
        Vector2Int gridCoord = grid != null ? grid.WorldToGrid(baseCellCenter) : Vector2Int.zero;

        if (!CanPlaceBuilding(gridCoord))
        {
            Debug.Log("Cannot place structure here, area is occupied. " + baseCellCenter);
            return;
        }

        // Prefab staje w ŚRODKU całego footprintu, a nie na bazowej komórce -
        // dzięki temu model wizualnie pokrywa dokładnie zajęte komórki.
        Vector3 spawnPos = FootprintCenter(gridCoord, prefabWidth, prefabHeight);
        GameObject placed = Instantiate(structurePrefab, spawnPos, Quaternion.identity);

        // Zapisz faktyczny rozmiar, żeby przy usuwaniu zwolnić właściwe komórki.
        BuildingFootprint footprint = placed.GetComponent<BuildingFootprint>();
        if (footprint == null)
            footprint = placed.AddComponent<BuildingFootprint>();
        footprint.Initialize(gridCoord, prefabWidth, prefabHeight);

        Debug.Log($"Placed structure at grid position: {gridCoord} ({prefabWidth}x{prefabHeight})");
        OccupyCells(gridCoord, prefabWidth, prefabHeight, true);

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

    /// Zmienia stan zajętości komórek pod budynkiem o zadanym rozmiarze.
    public void OccupyCells(Vector2Int baseCoord, int width, int height, bool state)
    {
        if (grid == null)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2Int coord = baseCoord + new Vector2Int(x, z);
                Cell cell = grid.GetCell(coord);

                if (cell != null)
                    cell.isOccupied = state;
            }
        }
    }

    /// <summary>Środek footprintu (środek komórki bazowej i skrajnej, średnia).</summary>
    private Vector3 FootprintCenter(Vector2Int baseCoord, int width, int height)
    {
        if (grid == null)
            return Vector3.zero;

        Vector3 from = grid.GridToWorld(baseCoord);
        Vector3 to = grid.GridToWorld(baseCoord + new Vector2Int(width - 1, height - 1));
        return (from + to) * 0.5f;
    }

    /// <summary>Rozmiar jednej komórki w świecie (osiowe, bez Y).</summary>
    private Vector3 CellWorldSize(Vector2Int coord)
    {
        if (grid == null)
            return Vector3.one;

        Vector3 origin = grid.GridToWorld(coord);
        Vector3 alongX = grid.GridToWorld(coord + Vector2Int.right) - origin;
        Vector3 alongZ = grid.GridToWorld(coord + Vector2Int.up) - origin;
        return new Vector3(Mathf.Abs(alongX.x), 0f, Mathf.Abs(alongZ.z));
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

        // Trafiony kolider może być na dziecku - szukamy struktury od rodzica w górę.
        Component structureComponent = FindStructureComponent(hit.collider);
        if (structureComponent == null)
        {
            Debug.Log($"'{hit.collider.name}' nie jest strukturą – nie usuwam.");
            return;
        }

        GameObject structure = structureComponent.gameObject;

        // Rozmiar footprintu bierzemy z obiektu, który faktycznie usuwamy,
        // a nie z aktualnie wybranego w menu prefabu.
        BuildingFootprint footprint = structure.GetComponent<BuildingFootprint>();
        StructureSize structureSize = structure.GetComponent<StructureSize>();

        Vector2Int gridCoord;
        int width = 1;
        int height = 1;

        if (footprint != null)
        {
            gridCoord = footprint.Origin;
            width = footprint.Width;
            height = footprint.Height;
        }
        else
        {
            gridCoord = grid != null ? grid.WorldToGrid(structure.transform.position) : Vector2Int.zero;
            if (structureSize != null)
            {
                width = structureSize.width;
                height = structureSize.height;
            }
        }

        OccupyCells(gridCoord, width, height, false);
        Destroy(structure);
        Debug.Log($"Usunięto '{structure.name}' z komórek: {gridCoord} ({width}x{height})");

        if (navMeshManager != null)
            navMeshManager.RebuildNavMesh();
    }

    /// <summary>Zwraca komponent oznaczający obiekt jako strukturę (albo null).</summary>
    private static Component FindStructureComponent(Collider collider)
    {
        foreach (System.Type type in StructureComponentTypes)
        {
            Component component = collider.GetComponentInParent(type);
            if (component != null)
                return component;
        }

        return null;
    }

    /// <summary>
    /// Zajmuje komórki pod budynkami, które już stoją w scenie (postawione ręcznie
    /// lub wczytane) - gracz nie może w nie budować. Idempotentne: po ponownej
    /// budowie siatki po prostu ustawia komórki jeszcze raz.
    /// </summary>
    private void OccupyExistingStructures()
    {
        if (grid == null)
            return;

        HashSet<GameObject> processed = new HashSet<GameObject>();

        foreach (System.Type type in StructureComponentTypes)
        {
            foreach (Component component in FindObjectsByType(type))
            {
                if (!processed.Add(component.gameObject))
                    continue;

                BuildingFootprint footprint = component as BuildingFootprint;
                if (footprint == null)
                    footprint = component.GetComponent<BuildingFootprint>();

                Vector2Int origin;
                int width = 1;
                int height = 1;

                if (footprint != null)
                {
                    origin = footprint.Origin;
                    width = footprint.Width;
                    height = footprint.Height;
                }
                else
                {
                    origin = grid.WorldToGrid(component.transform.position);

                    StructureSize size = component.GetComponent<StructureSize>();
                    if (size != null)
                    {
                        width = size.width;
                        height = size.height;
                    }
                }

                OccupyCells(origin, width, height, true);
            }
        }
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
            CreateOrUpdateHighlight(gridCoordHighlight);
            lastGridPos = gridCoordHighlight;
        }

        bool isValid = CanPlaceBuilding(gridCoordHighlight);
        UpdateHighlightColor(isValid);
    }

    private void CreateOrUpdateHighlight(Vector2Int gridCoord)
    {
        if (currentHighlight == null)
            CreateHighlightObject();

        // Highlight centrujemy na środku footprintu i skalujemy do jego realnego
        // rozmiaru w świecie - pokrywa dokładnie te same komórki, które zajmujemy.
        currentHighlight.transform.position = FootprintCenter(gridCoord, prefabWidth, prefabHeight);

        Vector3 cellSize = CellWorldSize(gridCoord);
        currentHighlight.transform.localScale = new Vector3(
            prefabWidth * cellSize.x,
            0.1f,
            prefabHeight * cellSize.z);
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
