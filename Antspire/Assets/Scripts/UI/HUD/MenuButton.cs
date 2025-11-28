using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    private Button button;
    private HUDManager hudManager;
    [SerializeField] SaveManager saveManager;

    void Start()
    {
        button = GetComponent<Button>();
        hudManager = FindAnyObjectByType<HUDManager>();
        saveManager = FindAnyObjectByType<SaveManager>();
        if (button == null)
        {
            Debug.LogError("MenuButton script requires a Button component on the same GameObject.", this);
        }
        if (button != null && hudManager != null && saveManager != null)
        {
            // Przypisz odpowiednie metody w zale¿noœci od nazwy przycisku
            switch (gameObject.name)
            {
                case string name when name.Contains("MenuButton"):
                    button.onClick.AddListener(hudManager.ToggleMenu);
                    break;

                case string name when name.Contains("Resume"):
                    button.onClick.AddListener(hudManager.ResumeGame);
                    break;

                case string name when name.Contains("Save"):
                    button.onClick.AddListener(saveManager.SaveGame);
                    break;

                case string name when name.Contains("Leave"):
                    button.onClick.AddListener(hudManager.LeaveGame);
                    break;

                case string name when name.Contains("Cancel"):
                    button.onClick.AddListener(hudManager.HideObjectInfo);
                    break;
                case string name when name.Contains("Load"):
                    button.onClick.AddListener(hudManager.LoadGame);
                    break;
            }
        }
    }
}