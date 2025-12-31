using UnityEngine;

public class Ant : MonoBehaviour, ISaveable
{
    public string antName;
    public AntState currentState;
    public Vector2Int gridPosition;
    public Vector2Int targetPosition;
    public House assignedHouse;
    public Workplace assignedWorkplace;
    public int health;
    public int carriedResources;
    Vector3 lastPosition;

    SaveManager saveManager;
    // mo¿emy to bezproblemowo zwiêkszyæ
    // aby zrobiæ load to trzeba dodaæ settery do pozosta³ych zmiennych

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SaveManager.Register(this);
        antName = gameObject.name;

        saveManager = FindAnyObjectByType<SaveManager>();
        if (saveManager == null )
        {             
            Debug.LogError("SaveManager not found in the scene!");
        }
        SetPosition();
    }

    // Update is called once per frame
    void Update() 
    {
        if (transform.position != lastPosition)
        {
            SetPosition();
        }
    }

    public void AssignHouse(House house)
    {
        assignedHouse = house;
        Debug.Log($"Ant {name} assigned to house {house}");
    }
    public void AssignWorkplace(Workplace workplace)
    {
        assignedWorkplace = workplace;
        workplace.CurrentEmployees += 1;
        Debug.Log($"Ant {name} assigned to workplace {workplace.WorkplaceName}");
    }
    public void SetState(AntState newState)
    {
        currentState = newState;
        //Debug.Log($"Ant {name} changed state to {newState}");
    }
    
    public void SetPosition()
    {
        gridPosition = saveManager.GetCurrentPosition(transform.position);  
        lastPosition = transform.position;
    }
    // ISaveable
    public object GetSaveData()
    {
        // Tworzysz strukturê danych, któr¹ zapiszesz w GameSave
        return new AntData
        {
            name = antName,
            position = gridPosition,
            state = currentState,
            assignedHouse = assignedHouse,
            assignedWorkplace = assignedWorkplace,
            health = health,
        };
    }
}