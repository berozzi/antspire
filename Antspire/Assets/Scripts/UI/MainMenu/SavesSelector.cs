using UnityEngine;
using UnityEngine.UI;

public class SavesSelector : MonoBehaviour
{
    Button slot;
    [SerializeField] MainMenuManager mainMenuManager;
    private void Start()
    {
        slot = GetComponent<Button>();
    }

    private void Update()
    {
        if (slot == null)
        {
            Debug.LogWarning("Button component not found on SavesSelector GameObject.");
        }
    }
}
