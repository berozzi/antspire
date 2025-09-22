using UnityEngine;
using UnityEngine.InputSystem;

public class HexClick : MonoBehaviour
{
    GenerateGrid gridGenerator;
    [SerializeField] Camera mainCamera;

    // Input Actions
    private InputAction leftClickAction;
    private bool isSubscribed = false;

    private void Awake()
    {
        // Tworzymy domyœln¹ akcjê klikniêcia
        leftClickAction = new InputAction("LeftClick", binding: "<Mouse>/leftButton");
    }

    void Start()
    {
        // ZnajdŸ gridGenerator jeœli nie jest przypisany
        if (gridGenerator == null)
            gridGenerator = FindAnyObjectByType<GenerateGrid>();
    }

    void OnEnable()
    {
        if (leftClickAction != null)
        {
            if (!isSubscribed)
            {
                leftClickAction.performed += OnClick;
                isSubscribed = true;
            }
            leftClickAction.Enable();
        }
    }

    void OnDisable()
    {
        if (leftClickAction != null && isSubscribed)
        {
            leftClickAction.performed -= OnClick;
            leftClickAction.Disable();
            isSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (leftClickAction != null)
        {
            if (isSubscribed)
            {
                leftClickAction.performed -= OnClick;
                isSubscribed = false;
            }
            leftClickAction.Dispose();
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        HexCell cell = ReturnTargetHexcell();
        if (cell != null)
        {
            Debug.Log($"Klikn¹³eœ hex: {cell.xPosition}, {cell.zPosition}");

            if (gridGenerator != null)
            {
                Vector3 worldPos = gridGenerator.CalculateWorldPosition(cell.xPosition, cell.zPosition, gridGenerator.hexSize);
                // TUTAJ KOD OD MROWKI ZEBY POSZLA DO CELU ZIOMA
            }
        }
    }
    public HexCell ReturnTargetHexcell()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedHex = hit.collider.gameObject;
            if (clickedHex != null)
            {
                HexCell tile = clickedHex.GetComponent<HexCell>();

                return tile;
            }
            // Wersja z komponentem HexCell
            return null;
        }
        return null;
    }
}