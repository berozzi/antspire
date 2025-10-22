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
        Camera cam = Camera.main;
        // punkt na "p³aszczyŸnie" kamery odpowiadaj¹cy kursorowi daleko od kamery
        float depth = cam.orthographic ? cam.farClipPlane : 100f; // dla perspective: daj wystarczaj¹co du¿e depth
        Vector3 origin = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
        Ray ray = new Ray(origin, cam.transform.forward * -1f); // w zale¿noœci od orientacji kamery mo¿esz potrzebowaæ -forward

        int layerMask = ~LayerMask.GetMask("Ground"); // ignoruj warstwê Ground
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            Vector3 buildingWorldPos = hitObject.transform.position;

            // U¯YJ TEJ SAMEJ KONWERSJI CO W PLACEBUILDING!
            Vector2Int gridCoord = grid.WorldToGrid(buildingWorldPos);
            
            if (placeDownStructure == null)
            {
                Debug.LogError("PlaceDownStructure reference is missing in DeleteStructure script.", this);
                return;
            }
            // Zmieñ stan komórki na nie zajêt¹
            placeDownStructure.ChangeOccupiedState(gridCoord, false);
            Destroy(hitObject);
            Debug.Log($"Deleted structure at grid position: {gridCoord}");
        }
    }
}