using UnityEngine;
using UnityEngine.UI;

public class StructureCategoriesSelect : MonoBehaviour
{
    private Button button;
    [SerializeField] GameplayCanvasManager gameplayCanvasManager;

    void Start()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("This script requires a Button component on the same GameObject.", this);
            return;
        }

        if (gameplayCanvasManager == null)
            gameplayCanvasManager = FindAnyObjectByType<GameplayCanvasManager>();

        if (gameplayCanvasManager == null)
        {
            Debug.LogError("GameplayCanvasManager not found in the scene.", this);
            return;
        }

        // Przypisz odpowiednie metody w zale�no�ci od nazwy przycisku
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
