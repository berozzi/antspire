using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceDownStructure : MonoBehaviour
{
    InputAction buildAction; // przypisz tê akcjê w inspektorze
    GameObject structurePrefab; // Przypisz instancjê GameSave w inspektorze
    GenerateVirtualGrid grid;
    HUDManager hudManager;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Highlight Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    private GameObject currentHighlight;
    private MeshRenderer highlightRenderer;
    private Vector2Int lastGridPos;
    private bool wasValidLastFrame = true;

    // te dictionary bêdzie publiczne i dostêpne dla innych skryptów do sprawdzania zajêtoœci pozycji
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

        // Ukryj podœwietlenie gdy skrypt jest wy³¹czony
        if (currentHighlight != null)
        {
            currentHighlight.SetActive(false);
        }
    }

    private void OnEnable()
    {
        buildAction.Enable();
        buildAction.performed += OnBuild;

        // Poka¿ podœwietlenie gdy skrypt jest w³¹czony
        if (currentHighlight != null)
        {
            currentHighlight.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Posprz¹taj po sobie
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
        // Pobierz pozycjê myszy w œwiecie
        Vector3 mousePos = GetMouseWorldPosition();

        // Przyci¹gnij do gridu
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
        // Zak³adaj¹c, ¿e masz dostêp do siatki i jej komórek
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
            //id = IncrementID(id),  // Musisz mieæ system ID
            type = structurePrefab.name, // lub pobierz z komponentu
            x = position.x,
            y = position.y,
            level = 1, // domyœlny poziom
            capacity = 10, // domyœlna pojemnoœæ
            isPlayerStructure = true // lub false w zale¿noœci od logiki gry
        };

        // Dodaj do listy aktywnych struktur
        gameSave.structures.Add(structureData);
        //Debug.Log($"Zapisano strukturê: {structureData.type} na pozycji ({position.x}, {position.y})");
    }

    Vector3 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        // punkt na "p³aszczyŸnie" kamery odpowiadaj¹cy kursorowi daleko od kamery
        float depth = cam.orthographic ? cam.farClipPlane : 100f; // dla perspective: daj wystarczaj¹co du¿e depth
        Vector3 origin = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
        Ray ray = new Ray(origin, cam.transform.forward * -1f); // w zale¿noœci od orientacji kamery mo¿esz potrzebowaæ -forward

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            return hit.point;
        }

        Debug.Log("Physics raycast nie trafi³.");
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

        // SprawdŸ czy pozycja siê zmieni³a
        if (gridCoordHighlight != lastGridPos || currentHighlight == null)
        {
            CreateOrUpdateHighlight(gridPosHighlight, gridCoordHighlight);
            lastGridPos = gridCoordHighlight;
        }

        // Aktualizuj kolor na podstawie dostêpnoœci
        bool isValid = !IsCellOccupied(gridCoordHighlight);
        UpdateHighlightColor(isValid);
    }

    private void CreateOrUpdateHighlight(Vector3 worldPos, Vector2Int gridCoord)
    {
        if (currentHighlight == null)
        {
            CreateHighlightObject();
        }

        // Ustaw pozycjê podœwietlenia
        currentHighlight.transform.position = worldPos;

        // Dopasuj rozmiar do twojego budynku
        float buildingSize = GetBuildingSize();
        currentHighlight.transform.localScale = new Vector3(buildingSize, 0.1f, buildingSize);
    }

    private void CreateHighlightObject()
    {
        currentHighlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentHighlight.name = "BuildingHighlight";

        // Usuñ collider ¿eby nie blokowa³ raycast
        Destroy(currentHighlight.GetComponent<Collider>());

        highlightRenderer = currentHighlight.GetComponent<MeshRenderer>();

        // Ustaw przezroczysty material domyœlnie
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
        // Mo¿esz pobraæ z prefaba lub ustawiæ sta³¹
        if (structurePrefab != null)
        {
            Renderer renderer = structurePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size.x * 0.9f; // 90% rozmiaru budynku
            }
        }
        return 0.9f; // domyœlny rozmiar
    }
}