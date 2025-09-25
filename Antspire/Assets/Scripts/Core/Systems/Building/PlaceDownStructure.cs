using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceDownStructure : MonoBehaviour
{
    float cellSize = 1f; // rozmiar komórki gridu
    InputAction buildAction; // przypisz tê akcjê w inspektorze
    public GameObject structurePrefab;

    private void Awake()
    {
        buildAction = new InputAction("LeftClick", binding: "<Mouse>/leftButton");
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
        PlaceBuilding();
    }
    void PlaceBuilding()
    {
        // Pobierz pozycjê myszy w œwiecie
        Vector3 mousePos = GetMouseWorldPosition();

        // Przyci¹gnij do gridu
        Vector3 gridPos = SnapBuildingToGrid(mousePos);

        // Stwórz budynek
        Instantiate(structurePrefab, gridPos, Quaternion.identity);
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
        float halfCell = cellSize * 0.5f;
        int x = Mathf.RoundToInt((worldPos.x - halfCell) / cellSize);
        int z = Mathf.RoundToInt((worldPos.z - halfCell) / cellSize);

        return new Vector3(x * cellSize + halfCell, worldPos.y, z * cellSize + halfCell);
    }
}