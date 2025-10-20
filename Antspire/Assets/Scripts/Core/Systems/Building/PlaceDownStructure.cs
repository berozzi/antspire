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
    private Dictionary<Vector2Int, bool> occupiedGridPositions = new Dictionary<Vector2Int, bool>();

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
    private void OnEnable()
    {
        buildAction.Enable();
        buildAction.performed += OnBuild; // rejestrujesz event
    }
    private void OnDisable()
    {
        buildAction.performed -= OnBuild;
        buildAction.Disable();
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

        if (IsPositionOccupied(gridCoord))
        {
            Debug.Log("Cannot place structure here, area is occupied.");
            return;
        }
        // Stwórz budynek

        Instantiate(structurePrefab, gridPos, Quaternion.identity);
        SetPositionOccupied(gridCoord, true);
        SaveStructureData(gridPos);
    }
    public bool IsPositionOccupied(Vector2Int gridCoord)
    {
        return occupiedGridPositions.ContainsKey(gridCoord) && occupiedGridPositions[gridCoord];
    }
    // ustawienie pozycji jako wolnej lub zajêtej
    public void SetPositionOccupied(Vector2Int gridCoord, bool occupied)
    {
        occupiedGridPositions[gridCoord] = occupied;
        Debug.Log($"Position {gridCoord} occupied status set to {occupied}");
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    public Vector3 SnapBuildingToGrid(Vector3 worldPos)
    {
        Vector2Int gridCoord = grid.WorldToGrid(worldPos);
        return grid.GridToWorld(gridCoord);
    }
}