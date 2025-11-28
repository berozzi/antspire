using UnityEngine;

public class Ant : MonoBehaviour, ISaveable
{
    public string antName;
    public AntState currentState;
    public Vector3 gridPosition;
    public Vector2Int targetPosition;
    public House assignedHouse;
    public Workplace assignedWorkplace;
    public int health;
    public int carriedResources;
    // mo¿emy to bezproblemowo zwiêkszyæ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SaveManager.Register(this);
        antName = gameObject.name;
    }

    // Update is called once per frame
    void Update() { }

    public void AssignHouse(House house)
    {
        assignedHouse = house;
        Debug.Log($"Ant {name} assigned to house {house}");
    }
    public void AssignWorkplace(Workplace workplace)
    {
        assignedWorkplace = workplace;
        Debug.Log($"Ant {name} assigned to workplace {workplace.WorkplaceName}");
    }
    public void SetState(AntState newState)
    {
        currentState = newState;
        Debug.Log($"Ant {name} changed state to {newState}");
    }
    Vector2Int GetCurrentPosition(Vector3 pos)
    {
        GenerateVirtualGrid gridManager = FindAnyObjectByType<GenerateVirtualGrid>();
        if (gridManager != null)
        { 
            return gridManager.WorldToGrid(pos);
        } else
        {
            Debug.LogError("GridManager not found!");
            return Vector2Int.zero;
        }      
    }
    // ISaveable
    public object GetSaveData()
    {
        // Tworzysz strukturê danych, któr¹ zapiszesz w GameSave
        return new AntData
        {
            name = antName,
            position = GetCurrentPosition(transform.position),
            state = currentState,
            assignedHouse = assignedHouse,
            assignedWorkplace = assignedWorkplace,
            health = health,
        };
    }
}