using UnityEngine;
using UnityEngine.InputSystem;

public class DeleteStructure : MonoBehaviour
{
    InputAction deleteAction;
    GenerateVirtualGrid grid;
    [SerializeField] PlaceDownStructure placeDownStructure;
    [SerializeField] HUDManager hudManager;
    private void Awake()
    {
        deleteAction = new InputAction("Delete", binding: "<Mouse>/leftButton");
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
        if  (hudManager != null && hudManager.isPaused)
        {
            // Nie usuwaj struktur, gdy gra jest wstrzymana
            return;
        }
        DeleteBuilding();
    }
    void DeleteBuilding()
    {
        // Użyj tej samej metody co w PlaceBuilding
        var (success, mouseWorldPos) = placeDownStructure.GetMouseWorldPosition();

        // Jeśli kliknięcie nie trafiło w layer Ground — anuluj
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

        // Rzuć ray od kamery przez myszkę
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Ignoruj layer Ground — chcemy trafić obiekt stojący NA ziemi
        int layerMask = ~LayerMask.GetMask("Ground");
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Dla bezpieczeństwa — jeśli z jakiegoś powodu klikniesz Ground mimo maski
            if (hitObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Debug.Log("Kliknięto Ground – brak obiektu do usunięcia.");
                return;
            }

            Vector3 buildingWorldPos = hitObject.transform.position;
            Vector2Int gridCoord = grid.WorldToGrid(buildingWorldPos);

            if (placeDownStructure == null)
            {
                Debug.LogError(" Brakuje referencji do placeDownStructure w DeleteBuilding.", this);
                return;
            }

            // Zmień stan komórki na wolną (jeśli istniała)
            placeDownStructure.ChangeOccupiedState(gridCoord, false);
            RemoveStructureFromSave(hitObject.transform.position);
            // Usuń obiekt
            Destroy(hitObject);
            Debug.Log($" Usunięto obiekt z koliderem na pozycji siatki: {gridCoord}");
        }
        else
        {
            Debug.Log("Nie trafiono żadnego obiektu z koliderem do usunięcia.");
        }
    }
    public void RemoveStructureFromSave(Vector3 exactPosition)
    {
        GameSave gameSave = GameSave.Instance;
        if (gameSave == null) return;

        // Szukaj dokładnie na tej pozycji (bez tolerancji, bo masz raycast)
        StructureData structureToRemove = gameSave.structures.Find(s =>
            Mathf.Approximately(s.x, exactPosition.x) &&
            Mathf.Approximately(s.y, exactPosition.y)
        );

        if (structureToRemove != null)
        {
            gameSave.structures.Remove(structureToRemove);
            Debug.Log($"Usunięto z zapisu: {structureToRemove.type} na ({exactPosition.x:F2}, {exactPosition.y:F2})");
        }
    }
}