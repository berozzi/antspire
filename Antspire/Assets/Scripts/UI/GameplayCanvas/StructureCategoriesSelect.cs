using UnityEngine;
using UnityEngine.UI;

public class StructureCategoriesSelect : MonoBehaviour
{
    private Button button;
    [SerializeField] GameplayCanvasManager gameplayCanvasManager;

    void Start()
    {
        button = GetComponent<Button>();

        if (gameplayCanvasManager == null)
        {
            gameplayCanvasManager = FindAnyObjectByType<GameplayCanvasManager>();
            Debug.LogError("GameplayCanvasManager not found in the scene.", this);
        }
        if (button == null)
        {
            Debug.LogError("This script requires a Button component on the same GameObject.", this);
        }
        // Przypisz odpowiednie metody w zale¿noœci od nazwy przycisku
        switch (gameObject.name)
        {
            case string name when name.Contains("Common"):
                button.onClick.AddListener(gameplayCanvasManager.ToggleCommonStructures);
                break;

            case string name when name.Contains("Health"):
                button.onClick.AddListener(gameplayCanvasManager.ToggleHealthStructures);
                break;

            case string name when name.Contains("Tunnels"):
                button.onClick.AddListener(gameplayCanvasManager.ToggleTunnelMenu);
                break;
        }
    }
}
