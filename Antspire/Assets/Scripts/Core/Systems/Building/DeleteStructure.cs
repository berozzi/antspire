using UnityEngine;
using UnityEngine.InputSystem;

public class DeleteStructure : MonoBehaviour
{
    InputAction deleteAction;
    GenerateVirtualGrid grid;
    [SerializeField] PlaceDownStructure placeDownStructure;
    private void Awake()
    {
        deleteAction = new InputAction("RightClick", binding: "<Mouse>/leftButton");
        grid = FindAnyObjectByType<GenerateVirtualGrid>();
        if (placeDownStructure == null)
        {
            Debug.LogError("PlaceDownStructure reference is missing in DeleteStructure script.", this);
        }
    }
    private void OnEnable()
    {
        deleteAction.Enable();
        deleteAction.performed += OnDelete; // rejestrujesz event
    }
    private void OnDisable()
    {
        deleteAction.performed -= OnDelete;
        deleteAction.Disable();
    }
    private void OnDelete(InputAction.CallbackContext context)
    {
        DeleteBuilding();
    }
    void DeleteBuilding()
    {
        // Pobierz pozycjê myszy w œwiecie
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        int layerMask = ~LayerMask.GetMask("Ground"); // ignoruj warstwê Ground
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            Vector3 buildingWorldPos = hitObject.transform.position;

            // U¯YJ TEJ SAMEJ KONWERSJI CO W PLACEBUILDING!
            Vector2Int gridCoord = grid.WorldToGrid(buildingWorldPos);
            
            if (placeDownStructure.IsPositionOccupied(gridCoord))
            {
                Destroy(hitObject);
                placeDownStructure.SetPositionOccupied(gridCoord, false);
                Debug.Log($"Building at Grid {gridCoord} deleted successfully");
            }
        }
    }
}