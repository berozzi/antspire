using UnityEngine;
using UnityEngine.UI;

public class SavesSelector : MonoBehaviour
{
    Button slot;
    [SerializeField] MainMenuManager mainMenuManager;

    private void Start()
    {
        slot = GetComponent<Button>();

        // Ostrzeżenie raz przy starcie, a nie w Update (co klatka = zalanie konsoli).
        if (slot == null)
            Debug.LogWarning("Button component not found on SavesSelector GameObject.", this);
    }
}
