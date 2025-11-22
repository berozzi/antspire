using UnityEngine;
using UnityEngine.UI;

public class SelectStructure : MonoBehaviour
{
    [SerializeField] private GameObject buildingPrefab;
    Button button;

    [SerializeField] private PlaceDownStructure placeDownStructure;
    [SerializeField] private GameState gameState;
    [SerializeField] private GameplayCanvasManager gameplayCanvas;

    void Start()
    {
        button = GetComponent<Button>();
        placeDownStructure = FindAnyObjectByType<PlaceDownStructure>();
        gameState = FindAnyObjectByType<GameState>();

        CheckMissingReferences();

        button.onClick.AddListener(OnBuildingSelected);
    }

    private void OnBuildingSelected()
    {
        if (placeDownStructure != null && buildingPrefab != null)
        {
            Debug.Log($"Selected building prefab: {buildingPrefab.name}");
            gameState.SetPrefab(buildingPrefab);
            gameState.TogglePanel(GameStates.BuildMode);
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
        if (placeDownStructure == null)
        {
            Debug.LogError("SelectStructure: PlaceDownStructure reference is missing.", this);
        }
        if (gameState == null)
        {
            Debug.LogError("SelectStructure: GameState reference is missing.", this);
        }
        if (gameplayCanvas == null)
        {
            Debug.LogError("SelectStructure: GameplayCanvasManager reference is missing.", this);
        }
    }
}
