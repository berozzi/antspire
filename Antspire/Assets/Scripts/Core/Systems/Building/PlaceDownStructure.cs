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

   
    private void Awake()
    {
        buildAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        grid = FindAnyObjectByType<GenerateVirtualGrid>();
        hudManager = FindAnyObjectByType<HUDManager>();
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    public void SetStructurePrefab(GameObject prefab)
    {
        structurePrefab = prefab;
    }
    private void Update()
    {
        if (structurePrefab == null) return;

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
    void Start()
    {
        if (structurePrefab == null)
        {
            Debug.LogError("structurePrefab is not assigned. Should be assigned later, in SelectStructure");
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

        if (IsCellOccupied(gridCoord))
        {
            Debug.Log("Cannot place structure here, area is occupied.");
            return;
        }
        // Stwórz budynek

        Instantiate(structurePrefab, gridPos, Quaternion.identity);
        Debug.Log($"Placed structure at grid position: {gridCoord}");
        ChangeOccupiedState(gridCoord, true);
        SaveStructureData(gridPos);
    }
    bool IsCellOccupied(Vector2Int gridCoord)
    {
        // Zakładając, że masz dostęp do siatki i jej komórek
        Cell cell = grid.GetCell(gridCoord);
        if (cell == null)
        {
            Debug.LogError($"Cell at {gridCoord} is null.");
            
        }
        return cell != null && cell.isOccupied;
    }
    // zmiana stanu zajętości komórki
    public void ChangeOccupiedState(Vector2Int gridCoord, bool occupied)
    {
        Cell cell = grid.GetCell(gridCoord);
        if (cell != null)
        {
            cell.isOccupied = occupied;
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
        bool isValid = !IsCellOccupied(gridCoordHighlight);
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
        float buildingSize = GetBuildingSize();
        currentHighlight.transform.localScale = new Vector3(buildingSize, 0.1f, buildingSize);
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

    private float GetBuildingSize()
    {
        // Dostosuj rozmiar do twojego budynku
        // Możesz pobrać z prefaba lub ustawić stałą
        if (structurePrefab != null)
        {
            Renderer renderer = structurePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size.x * 0.9f; // 90% rozmiaru budynku
            }
        }
        return 0.9f; // domyślny rozmiar
    }

    // Zapisz dane struktury do GameSave
    void SaveStructureData(Vector3 position)
    {
        GameSave gameSave = GameSave.Instance;
        if (gameSave == null)
        {
            Debug.Log("GameSave is null");
        }
        // Utwórz nowy obiekt StructureData przed zapisaniem
        StructureData structureData = new StructureData()
        {
            //id = IncrementID(id),  // Musisz mieć system ID
            type = structurePrefab.name, // lub pobierz z komponentu
            x = position.x,
            y = position.y,
            level = 1, // domyślny poziom
            capacity = 10, // domyślna pojemność
            isPlayerStructure = true // lub false w zależności od logiki gry
        };

        // Dodaj do listy aktywnych struktur
        gameSave.structures.Add(structureData);
        //Debug.Log($"Zapisano strukturę: {structureData.type} na pozycji ({position.x}, {position.y})");
    }
}