using UnityEngine;
using UnityEngine.InputSystem;

public class LoadSystem : MonoBehaviour
{
    InputAction clickAction;
    string customPath = "C:\\Users\\GracjanCode\\Desktop\\Ants\\Saves\\gameSave.json";
    [SerializeField] GameObject clickedCube;
    GameSave loadedGame;
    private void Awake()
    {
        clickAction = new InputAction("LeftClick", binding: "<Mouse>/leftButton");
    }
    private void OnEnable()
    {
        clickAction.Enable();
        clickAction.performed += OnClick;
    }

    private void OnDisable()
    {
        clickAction.performed -= OnClick;
        clickAction.Disable();
    }
    // we will change this click event to a button press event later
    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject != clickedCube) return;
            LoadGame();
            Debug.Log("Game Loaded with New Input System!");
            Debug.Log(loadedGame.player.coins);
            Debug.Log(loadedGame.player.feromones);
            Debug.Log(loadedGame.resources.Count);
        }
    }

    GameSave LoadGame()
    {
        return loadedGame = JsonUtility.FromJson<GameSave>(System.IO.File.ReadAllText(customPath));
    }
}
