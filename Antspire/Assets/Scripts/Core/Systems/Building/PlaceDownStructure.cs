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
    BuildingData buildingData;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Highlight Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    private GameObject currentHighlight;
    private MeshRenderer highlightRenderer;
    private Vector2Int lastGridPos;
    private bool wasValidLastFrame = true;

    // te dictionary będzie publiczne i dostępne dla innych skryptów do sprawdzania zajętości pozycji
    private void Awake()
    {
        buildAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        grid = FindAnyObjectByType<GenerateVirtualGrid>();
        hudManager = FindAnyObjectByType<HUDManager>();
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
            Debug.LogError("Assign the structure which you want to build.");
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
        Vector3 mousePos = GetMouseWorldPosition();

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
        Debug.Log($"Cell at {gridCoord} occupied state: {cell.isOccupied}");
        if (cell == null)
        {
            Debug.LogError($"Cell at {gridCoord} is null.");
        }
        return cell != null && cell.isOccupied;
    }

    public void ChangeOccupiedState(Vector2Int gridCoord, bool occupied)
    {
        Cell cell = grid.GetCell(gridCoord);
        if (cell != null)
        {
            cell.isOccupied = occupied;
            //buildingData = new BuildingData(gridCoord, BuildingType.House, 10, true);
            Debug.Log($"Cell at {gridCoord} occupied state changed to {occupied}.");
        }
    }

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

    Vector3 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Camera.main is null!");
            return Vector3.zero;
        }

        // Unity ma wbudowaną metodę do tego!
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // DEBUG: Zobacz ray w Scene view (czerwona linia)
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            Debug.Log($"Raycast trafił w: {hit.point}");
            return hit.point;
        }

        Debug.LogWarning($" Raycast nie trafił! Layer mask: {groundLayerMask.value}");

        // FALLBACK: przecięcie z płaszczyzną Y=0
        float distance = -ray.origin.y / ray.direction.y;
        if (distance > 0)
        {
            Vector3 fallbackPos = ray.origin + ray.direction * distance;
            Debug.Log($"Użyto fallback position: {fallbackPos}");
            return fallbackPos;
        }

        return Vector3.zero;
    }

    public Vector3 SnapBuildingToGrid(Vector3 worldPos)
    {
        Vector2Int gridCoord = grid.WorldToGrid(worldPos);
        return grid.GridToWorld(gridCoord);
    }

    private void UpdateHighlight()
    {
        Vector3 mousePosHighlight = GetMouseWorldPosition();
        Vector3 gridPosHighlight = SnapBuildingToGrid(mousePosHighlight);
        Vector2Int gridCoordHighlight = grid.WorldToGrid(gridPosHighlight);

        // Sprawdź czy pozycja się zmieniła
        if (gridCoordHighlight != lastGridPos || currentHighlight == null)
        {
            CreateOrUpdateHighlight(gridPosHighlight, gridCoordHighlight);
            lastGridPos = gridCoordHighlight;
        }

        // Aktualizuj kolor na podstawie dostępności
        bool isValid = !IsCellOccupied(gridCoordHighlight);
        UpdateHighlightColor(isValid);
    }

    private void CreateOrUpdateHighlight(Vector3 worldPos, Vector2Int gridCoord)
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
    
}