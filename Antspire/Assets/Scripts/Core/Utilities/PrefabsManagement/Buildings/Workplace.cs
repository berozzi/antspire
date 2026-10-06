using UnityEngine;

public class Workplace : MonoBehaviour, ISaveable, IClickable
{
    [Header("Wydajność Pracy")]
    [SerializeField] private string workplaceName = "Miejsce Pracy";
    [SerializeField] private string description = "Opis miejsca pracy.";
    [SerializeField] private float baseIncomePerSecond = 1f;
    [SerializeField] private bool generatesIncome = true; // false, jeśli miejsce pracy nie generuje dochodu
    [SerializeField] private bool isActive = true;
    [SerializeField] private WorkplaceType workplaceType;
    [SerializeField] private int capacity = 1;
    [SerializeField] private int currentEmployees = 0;
    [SerializeField] private int level = 1;

    // Properties dla AI
    public string WorkplaceName => workplaceName;
    public string Description => description;
    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
    public float CurrentIncome => generatesIncome && isActive ? baseIncomePerSecond : 0f;
    public int Capacity => capacity;
    public int CurrentEmployees
    {
        get { return currentEmployees; }
        set { currentEmployees = value; }
    }
    public int Level
    {
        get { return level; }
        set { level = value; }
    }

    void Start()
    {
        SaveManager.Register(this);
    }

    /// Ulepsza wydajność tego miejsca pracy
    public void UpgradeIncome(float upgradeAmount)
    {
        baseIncomePerSecond += upgradeAmount;
        Debug.Log($"Ulepszono {workplaceName}. Nowa wydajność: {baseIncomePerSecond}/s");
    }

    /// Ustawia nową bazową wartość wydajności
    public void SetBaseIncome(float newIncome)
    {
        baseIncomePerSecond = newIncome;
    }

    int AvailableSpots()
    {
        return Capacity - CurrentEmployees;
    }

    public bool HasAvailableCapacity()
    {
        AvailableSpots();
        return CurrentEmployees < Capacity;
    }

    public object GetSaveData()
    {
        return new WorkplaceData
        {
            workplaceName = this.workplaceName,
            baseIncomePerSecond = this.baseIncomePerSecond,
            generatesIncome = this.generatesIncome,
            isActive = this.IsActive,
            workplaceType = this.workplaceType,
            capacity = this.capacity,
            level = this.level
        };
    }

    public void LoadDataFromSave(object data)
    {
        if (data is WorkplaceData workplaceData)
        {
            this.workplaceName = workplaceData.workplaceName;
            this.baseIncomePerSecond = workplaceData.baseIncomePerSecond;
            this.generatesIncome = workplaceData.generatesIncome;
            this.IsActive = workplaceData.isActive;
            this.workplaceType = workplaceData.workplaceType;
            this.capacity = workplaceData.capacity;
            this.Level = workplaceData.level;
        }
    }

    /// Aktywuje/dezaktywuje generowanie dochodu
    public void SetIncomeActive(bool active)
    {
        isActive = active;
        Debug.Log($"{workplaceName} - generowanie dochodu: {(active ? "AKTYWNE" : "WYŁĄCZONE")}");
    }

    void OnDestroy()
    {
        SaveManager.Unregister(this);
    }

    /// IClickable implementation
    public void OnClick()
    {
        Debug.Log($"Clicked on workplace: {workplaceName}");
    }

    /// Zwraca dane do wyświetlenia w UI po kliknięciu
    public ClickableData GetClickableData()
    {
        var stats = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Dochód", $"{CurrentIncome}/s" },
            { "Status", IsActive ? "Aktywne" : "Nieaktywne" },
            { "Pojemność", $"{CurrentEmployees}/{Capacity}" }
        };
        return new ClickableData
        {
            Name = workplaceName,
            Type = workplaceType.ToString(),
            Description = description,
            Level = this.Level,
            Icon = null, // Można przypisać ikonę miejsca pracy tutaj
            Stats = stats
        };
    }
}
