using Unity.AppUI.UI;
using UnityEngine;

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

        //switch (slot.Text.Text)
        //{
        //    case "Slot 1":
        //        if (PlayerPrefs.HasKey("Save1"))
        //        {
        //            mainMenuManager.LoadGame(1);
        //        }
        //        else
        //        {
        //            mainMenuManager.PlayGame(1);
        //        }
        //        break;
        //    case "Slot 2":
        //        if (PlayerPrefs.HasKey("Save2"))
        //        {
        //            mainMenuManager.LoadGame(2);
        //        }
        //        else
        //        {
        //            mainMenuManager.PlayGame(2);
        //        }
        //        break;
        //    case "Slot 3":
        //        if (PlayerPrefs.HasKey("Save3"))
        //        {
        //            mainMenuManager.LoadGame(3);
        //        }
        //        else
        //        {
        //            mainMenuManager.PlayGame(3);
        //        }
        //        break;
        //    default:
        //        Debug.LogWarning("Unknown slot selected.");
        //        break;
        //}
    }
}
