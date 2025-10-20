using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

public class SaveSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    InputAction clickAction;
    string customPath = "C:\\Users\\GracjanCode\\Desktop\\Ants\\Saves\\gameSave.json";
    [SerializeField] GameObject clickedCube;
    
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
        GameSave gameSave = GameSave.Instance;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject != clickedCube) return;
            
            gameSave.player = new PlayerData()
            {
                coins = 500,
                feromones = 300,
                wisdomPoints = 5
            };
            gameSave.resources.Add(new ResourceData()
            {
                id = System.Guid.NewGuid().ToString(),
                resourceName = "Wood",
                type = ResourceType.Wood,
                x = 10,
                y = 20,
                value = 100,
                isAvailable = true
            });

            SaveGame(gameSave);
        }
    }
    // this is working properly
    // also add encryptig script later
    public void SaveGame(GameSave gameSave)
    {
        string json = JsonUtility.ToJson(gameSave, true);
        File.WriteAllText(customPath, json);
        Debug.Log("Game Saved with New Input System!");
    }
}
