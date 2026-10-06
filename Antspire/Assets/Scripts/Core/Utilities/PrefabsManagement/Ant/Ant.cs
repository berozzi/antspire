using UnityEngine;

public class Ant : MonoBehaviour, ISaveable, IClickable
{
    public string antName;
    public Vector2Int gridPosition;
    public Vector2Int targetPosition;
    public House assignedHouse;
    public Workplace assignedWorkplace;
    public int health;
    public int carriedResources;
    Vector3 lastPosition;

    // Cache zamiast FindAnyObjectByType w każdej klatce ruchu - przy wielu
    // mrówkach przeszukiwanie sceny na klatkę było bardzo kosztowne.
    GridManager grid;
    // mo�emy to bezproblemowo zwi�kszy�
    // aby zrobi� load to trzeba doda� settery do pozosta�ych zmiennych

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SaveManager.Register(this);
        antName = gameObject.name;

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

    void OnDestroy()
    {
        // Zwalnia etat - inaczej miejsce pracy zostaje "pe�ne" na zawsze.
        if (assignedWorkplace != null)
        {
            assignedWorkplace.CurrentEmployees = Mathf.Max(0, assignedWorkplace.CurrentEmployees - 1);
            assignedWorkplace = null;
        }

        SaveManager.Unregister(this);
    }

    public void AssignHouse(House house)
    {
        assignedHouse = house;
        Debug.Log($"Ant {name} assigned to house {house}");
    }
    public void AssignWorkplace(Workplace workplace)
    {
        if (workplace == null)
            return;

        assignedWorkplace = workplace;
        workplace.CurrentEmployees += 1;
        Debug.Log($"Ant {name} assigned to workplace {workplace.WorkplaceName}");
    }
    //public void SetState(AntState newState)
    //{
    //    currentState = newState;
    //    //Debug.Log($"Ant {name} changed state to {newState}");
    //}
    
    public void SetPosition()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();

        if (grid != null)
            gridPosition = grid.WorldToGrid(transform.position);

        lastPosition = transform.position;
    }
    // ISaveable
    public object GetSaveData()
    {
        // Tworzysz struktur� danych, kt�r� zapiszesz w GameSave
        return new AntData
        {
            name = antName,
            position = gridPosition,
            //state = currentState,
            assignedHouse = assignedHouse,
            assignedWorkplace = assignedWorkplace,
            health = health,
        };
    }

    public void LoadDataFromSave(object data)
    {
        if (data is AntData antData)
        {
            antName = antData.name;
            gridPosition = antData.position;
            //currentState = antData.state;
            assignedHouse = antData.assignedHouse;
            assignedWorkplace = antData.assignedWorkplace;
            health = antData.health;
        }
    }

    /// IClickable

    public void OnClick()
    {
        Debug.Log($"Ant {antName} clicked.");
    }

    public ClickableData GetClickableData()
    {
        return new ClickableData
        {
            Name = this.antName,
            Type = "Ant",
            Description = $"An ant currently in state: ",
            Level = 1,
            Icon = null, // Assign appropriate icon here
            Stats = new System.Collections.Generic.Dictionary<string, string>
            {
                //{ "State", currentState.ToString() },
                { "Health", health.ToString() },
                { "Carried Resources", carriedResources.ToString() }
                // Add more stats as needed
            }
        };
    }
}