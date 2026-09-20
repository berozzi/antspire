using UnityEngine;
using UnityEngine.UI;

public class SelectStructure : MonoBehaviour
{
    [SerializeField] private GameObject buildingPrefab;
    Button button;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameplayCanvasManager gameplayCanvas;
    [SerializeField] private int buildingWidth = 1;
    [SerializeField] private int buildingHeight = 1;

    void Start()
    {
        button = GetComponent<Button>();
        gameManager = FindAnyObjectByType<GameManager>();

        CheckMissingReferences();

        button.onClick.AddListener(OnBuildingSelected);
    }

    private void OnBuildingSelected()
    {
        if (buildingPrefab != null)
        {
            Debug.Log($"Selected building prefab: {buildingPrefab.name}");
            gameManager.SetPrefab(buildingPrefab, buildingWidth, buildingHeight);
            gameManager.TogglePanel(GameStates.BuildMode);
            gameplayCanvas.CloseAllSubpanels();
        }
    }

    void CheckMissingReferences()
    {
        if (button == null)
        {
            Debug.LogError("SelectStructure: Button component is missing.", this);
        }
        if (buildingPrefab == null)
        {
            Debug.LogError("SelectStructure: Building prefab reference is missing.", this);
        }
        if (gameManager == null)
        {
            Debug.LogError("SelectStructure: GameManager reference is missing.", this);
        }
        if (gameplayCanvas == null)
        {
            Debug.LogError("SelectStructure: GameplayCanvasManager reference is missing.", this);
        }
    }
}