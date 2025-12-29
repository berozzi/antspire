using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceDownStructure : MonoBehaviour
{
    InputAction buildAction; // przypisz tę akcję w inspektorze
    GameObject structurePrefab; // Przypisz instancję GameSave w inspektorze
    GenerateVirtualGrid grid;
    HUDManager hudManager;
    LayerMask groundLayerMask;

    [Header("Highlight Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    private GameObject currentHighlight;
    private MeshRenderer highlightRenderer;
    private Vector2Int lastGridPos;
    private bool wasValidLastFrame = true;

    int prefabWidth;
    int prefabHeight;

    [SerializeField] NavMeshManager navMeshManager;
    private void Awake()
    {
        buildAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        grid = FindAnyObjectByType<GenerateVirtualGrid>();
        hudManager = FindAnyObjectByType<HUDManager>();
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    public void SetStructurePrefab(GameObject prefab, int w, int h)
    {
        structurePrefab = prefab;
        prefabWidth = w;
        prefabHeight = h;
    }
    private void Update()
    {
        if (structurePrefab == null)
        {
            Debug.Log("Structure prefab not assigned, highlight might not working");
            return;
        }

        UpdateHighlight();
    }

    private void OnDisable()
    {
        buildAction.performed -= OnBuild;
        buildAction.Disable();

        // Ukryj podświetlenie gdy skrypt jest wyłączony
        if (currentHighlight != null)
        {
            currentHighlight.SetActive(false);
        }
    }

    private void OnEnable()
    {
        buildAction.Enable();
        buildAction.performed += OnBuild;

        // Pokaż podświetlenie gdy skrypt jest włączony
        if (currentHighlight != null)
        {
            currentHighlight.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Posprzątaj po sobie
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }
    private void OnBuild(InputAction.CallbackContext context)
    {
        if (hudManager.isPaused) return; // Nie buduj, gdy gra jest wstrzymana
        PlaceBuilding();
    }
    void PlaceBuilding()
    {
        // Pobierz pozycję myszy w świecie
        var (success, mousePos) = GetMouseWorldPosition();
        if (!success)
        {
            Debug.Log("❌ Kliknięto poza Ground, nie stawiam budynku.");
            return;
        }

        // Przyciągnij do gridu
        Vector3 gridPos = SnapBuildingToGrid(mousePos);
        Vector2Int gridCoord = grid.WorldToGrid(gridPos);

        if (!CanPlaceBuilding(gridCoord))
        {
            Debug.Log("Cannot place structure here, area is occupied." + gridPos);
            return;
        }
        // Stwórz budynek
        Instantiate(structurePrefab, gridPos, Quaternion.identity);
        Debug.Log($"Placed structure at grid position: {gridCoord}");
        OccupyCells(gridCoord, true);
        // Refresh navmesh
        navMeshManager.RebuildNavMesh();
    }
    bool CanPlaceBuilding(Vector2Int baseCoord)
    {
        for (int x = 0; x < prefabWidth; x++)
        {
            for (int z = 0; z < prefabHeight; z++)
            {
                Vector2Int checkCoord = baseCoord + new Vector2Int(x, z);

                // sprawdź czy komórka istnieje
                Cell cell = grid.GetCell(checkCoord);
                if (cell == null)
                    return false;

                // sprawdź czy komórka zajęta
                if (cell.isOccupied)
                    return false;
            }
        }

        return true;
    }
    // zmiana stanu zajętości komórki
    public void OccupyCells(Vector2Int baseCoord, bool state)
    {
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
    // zbieranie pozycji w świecie pod myszką za pomocą raycasta, ten bool na początku i Vector3 to tuple, funckja zwraca dwie wartości bool i vector3
    public (bool success, Vector3 point) GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Camera.main is null!");
            return (false, Vector3.zero);
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            return (true, hit.point);
        }

        return (false, Vector3.zero);
    }
    // funkcja przyciągająca budynek do siatki
    // należy ją wykorzystać przy wczytywaniu budynków z zapisu
    public Vector3 SnapBuildingToGrid(Vector3 worldPos)
    {
        Vector2Int gridCoord = grid.WorldToGrid(worldPos);
        return grid.GridToWorld(gridCoord);
    }
    // aktualizacja podświetlenia pod budową budynku
    private void UpdateHighlight()
    {
        var (success, mousePosHighlight) = GetMouseWorldPosition();
        if (!success)
        {
            Debug.Log("❌ Kliknięto poza Ground, nie stawiam budynku.");
            return;
        }
        Vector3 gridPosHighlight = SnapBuildingToGrid(mousePosHighlight);
        Vector2Int gridCoordHighlight = grid.WorldToGrid(gridPosHighlight);

        // Sprawdź czy pozycja się zmieniła
        if (gridCoordHighlight != lastGridPos || currentHighlight == null)
        {
            CreateOrUpdateHighlight(gridPosHighlight);
            lastGridPos = gridCoordHighlight;
        }

        // Aktualizuj kolor na podstawie dostępności
        bool isValid = CanPlaceBuilding(gridCoordHighlight);
        UpdateHighlightColor(isValid);
    }
    // stworzenie highlightu lub jego aktualizacja
    private void CreateOrUpdateHighlight(Vector3 worldPos)
    {
        if (currentHighlight == null)
        {
            CreateHighlightObject();
        }

        // Ustaw pozycję podświetlenia
        currentHighlight.transform.position = worldPos;

        // Dopasuj rozmiar do twojego budynku
        
        currentHighlight.transform.localScale = new Vector3(prefabWidth, 0.1f, prefabHeight * 0.9f);
    }
    // stworzenie obiektu podświetlenia Cube'a
    private void CreateHighlightObject()
    {
        currentHighlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentHighlight.name = "BuildingHighlight";

        // Usuń collider żeby nie blokował raycast
        Destroy(currentHighlight.GetComponent<Collider>());

        highlightRenderer = currentHighlight.GetComponent<MeshRenderer>();

        // Ustaw przezroczysty material domyślnie
        highlightRenderer.material = highlightMaterial;
    }

    private void UpdateHighlightColor(bool isValid)
    {
        if (highlightRenderer == null) return;

        if (isValid != wasValidLastFrame)
        {
            highlightRenderer.material = isValid ? highlightMaterial : invalidMaterial;
            wasValidLastFrame = isValid;
        }
    }
}