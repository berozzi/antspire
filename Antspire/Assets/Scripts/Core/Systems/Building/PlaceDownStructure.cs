using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceDownStructure : MonoBehaviour
{
    [SerializeField] private InputAction buildAction; // przypisz tê akcjê w inspektorze
    HexClick hexClick;
    public GameObject structurePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        hexClick = FindAnyObjectByType<HexClick>();
    }

    private void OnBuild(InputAction.CallbackContext context)
    {
        TryPlaceStructure();
    }

    void TryPlaceStructure()
    {
        var cell = hexClick.ReturnTargetHexcell();
        var structurePosition = cell.transform.position;
        structurePosition.y += 1f;

        if (cell != null && structurePrefab != null)
        {
            Instantiate(structurePrefab, structurePosition, Quaternion.identity);
            Debug.Log($"Placed structure at hex: {cell.xPosition}, {cell.zPosition}");
        }
        else
        {
            Debug.Log("No cell or structurePrefab is null");
        }
    }
}
