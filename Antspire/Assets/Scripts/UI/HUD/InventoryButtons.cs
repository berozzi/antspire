using UnityEngine;
using UnityEngine.UI;

public class InventoryButtons : MonoBehaviour
{
    private Button button;
    [SerializeField] GameState gameState;
    [SerializeField] GameplayCanvasManager gameplayCanvasManager;

    void Start()
    {
        button = GetComponent<Button>();
        
        if (gameState == null)
        {
            gameState = FindAnyObjectByType<GameState>();
            Debug.LogError("GameState not found in the scene.", this);
        }
        if (button == null)
        {
            Debug.LogError("InventoryButtons script requires a Button component on the same GameObject.", this);
        }
        if (button != null && gameState != null)
        {
            // Przypisz odpowiednie metody w zale¿noœci od nazwy przycisku
            switch (gameObject.name)
            {
                case string name when name.Contains("Slot1"):
                    button.onClick.AddListener(() => gameState.TogglePanel(GameStates.TechTree));
                    break;

                case string name when name.Contains("Slot2"):
                    button.onClick.AddListener(() => gameState.TogglePanel(GameStates.WarPanel));
                    break;

                case string name when name.Contains("Slot3"):
                    button.onClick.AddListener(() => gameState.TogglePanel(GameStates.InStructureMenu));
                    // otworzenie menu z strukturasmi budowlanymi
                    break;

                case string name when name.Contains("Slot4"):
                    button.onClick.AddListener(() => gameState.TogglePanel(GameStates.DestroyMode));
                    // otworzenie menu z narzedziami do niszczenia struktur
                    break;
            }
        }
    }
}
