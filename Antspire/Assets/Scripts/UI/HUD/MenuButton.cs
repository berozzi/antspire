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
            return;
        }

        if (hudManager == null)
        {
            Debug.LogError("MenuButton: HUDManager not found in the scene.", this);
            return;
        }

        // Przypisz odpowiednie metody w zale�no�ci od nazwy przycisku.
        // Tylko Save/Load wymagaj� SaveManagera - reszta dzia�a tak�e bez niego
        // (wcze�niej brak SaveManagera odcina� CA�Y menu, ��cznie z Leave/Resume).
        switch (gameObject.name)
        {
            case string name when name.Contains("MenuButton"):
                button.onClick.AddListener(hudManager.ToggleMenu);
                break;

            case string name when name.Contains("Resume"):
                button.onClick.AddListener(hudManager.ResumeGame);
                break;

            case string name when name.Contains("Save"):
                if (saveManager != null)
                    button.onClick.AddListener(saveManager.SaveGame);
                else
                    Debug.LogError("MenuButton: SaveManager not found - SaveGame nie zostanie podpi�ty.", this);
                break;

            case string name when name.Contains("Leave"):
                button.onClick.AddListener(hudManager.LeaveGame);
                break;

            case string name when name.Contains("Cancel"):
                button.onClick.AddListener(hudManager.HideObjectInfo);
                break;

            case string name when name.Contains("Load"):
                if (saveManager != null)
                    button.onClick.AddListener(saveManager.LoadGame);
                else
                    Debug.LogError("MenuButton: SaveManager not found - LoadGame nie zostanie podpi�ty.", this);
                break;
        }
    }
}
