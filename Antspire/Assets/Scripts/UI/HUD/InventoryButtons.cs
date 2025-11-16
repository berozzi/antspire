using UnityEngine;
using UnityEngine.UI;

public class InventoryButtons : MonoBehaviour
{
    private Button button;
    [SerializeField] GameState gameState;

    void Start()
    {
        button = GetComponent<Button>();
        
        if (gameState == null)
        {
            gameState = FindAnyObjectByType<GameState>();
            Debug.LogError("GameplayCanvasManager not found in the scene.", this);
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
                    button.onClick.AddListener(gameState.ToggleTechTree);
                    break;

                case string name when name.Contains("Slot2"):
                    button.onClick.AddListener(gameState.ToggleWarPanel);
                    break;

                case string name when name.Contains("Slot3"):
                    // oteorzenie menu z strukturasmi budowlanymi
                    break;
            }
        }
    }
}
