using UnityEngine;
using UnityEngine.UI;

public class InventoryButtons : MonoBehaviour
{
    private Button button;
    [SerializeField] GameManager gameManager;

    void Start()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("InventoryButtons script requires a Button component on the same GameObject.", this);
            return;
        }

        // Przypisz odpowiednie metody w zależności od nazwy przycisku
        switch (gameObject.name)
        {
            case string name when name.Contains("Slot1"):
                button.onClick.AddListener(() => TogglePanel(GameStates.TechTree));
                break;

            case string name when name.Contains("Slot2"):
                button.onClick.AddListener(() => TogglePanel(GameStates.WarPanel));
                break;

            case string name when name.Contains("Slot3"):
                button.onClick.AddListener(() => TogglePanel(GameStates.InStructureMenu));
                // otworzenie menu z strukturami budowlanymi
                break;

            case string name when name.Contains("Slot4"):
                button.onClick.AddListener(() => TogglePanel(GameStates.DestroyMode));
                // otworzenie menu z narzędziami do niszczenia struktur
                break;
        }
    }

    private void TogglePanel(GameStates state)
    {
        // Leniwe wyszukanie - GameManager może być dostępny dopiero w trakcie gry.
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("InventoryButtons: GameManager not found in the scene.", this);
            return;
        }

        gameManager.TogglePanel(state);
    }
}