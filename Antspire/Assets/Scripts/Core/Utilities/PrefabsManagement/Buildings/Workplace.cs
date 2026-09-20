using UnityEngine;

public class Workplace : MonoBehaviour, ISaveable, IClickable
{
    [Header("Doch�d Pasywny")]
    [SerializeField] private string workplaceName = "Miejsce Pracy";
    [SerializeField] private string description = "Opis miejsca pracy.";
    [SerializeField] private float baseIncomePerSecond = 1f;
    [SerializeField] private bool generatesIncome = true; // set to false if this workplace does not generate pheromone income
    [SerializeField] private bool isActive = true;
    [SerializeField] private WorkplaceType workplaceType;
    [SerializeField] private int capacity = 1;
    [SerializeField] private int currentEmployees = 0;
    [SerializeField] private int level = 1;

    [Header("Referencje")]
    [SerializeField] private ProductionManager productionManager;

    private PassiveIncomeSource incomeSource;
    private bool isRegistered = false;


    // Properties dla AI
    public string WorkplaceName => workplaceName;
    public string Description => description;
    public bool IsActive { 
        get { return isActive; } 
        set { isActive = value; }
    }
    public float CurrentIncome => incomeSource?.GetIncomePerSecond() ?? 0f;
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
        InitializeIncomeSource();
    }

    void InitializeIncomeSource()
    {
        if (!generatesIncome) return;

        if (productionManager == null)
            productionManager = FindAnyObjectByType<ProductionManager>();

        // Utw�rz �r�d�o dochodu
        incomeSource = new PassiveIncomeSource(workplaceName, baseIncomePerSecond);

        // Zarejestruj si� w managerze
        if (productionManager != null)
        {
            productionManager.RegisterPassiveSource(incomeSource);
            isRegistered = true;
        }
        else
        {
            Debug.LogWarning($"ProductionManager nie przypisany do Workplace: {workplaceName}");
        }
    }

    /// Ulepsza doch�d z tego miejsca pracy
    public void UpgradeIncome(float upgradeAmount)
    {
        if (incomeSource != null)
        {
            incomeSource.UpgradeBaseIncome(upgradeAmount);
            Debug.Log($"Ulepszono {workplaceName}. Nowy doch�d: {incomeSource.GetIncomePerSecond()}/s");
        }
    }
   
    /// Ustawia now� bazow� warto�� dochodu
    public void SetBaseIncome(float newIncome)
    {
        if (incomeSource != null)
        {
            // Mo�emy doda� logik� obliczania r�nicy i aktualizacji
            baseIncomePerSecond = newIncome;
            // Tutaj potrzebowaliby�my metody do aktualizacji w PassiveIncomeSource
        }
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
        if (incomeSource != null)
        {
            incomeSource.SetActive(active);
            Debug.Log($"{workplaceName} - generowanie dochodu: {(active ? "AKTYWNE" : "WY��CZONE")}");
        }
    }
    void OnDestroy()
    {
        SaveManager.Unregister(this);
        if (productionManager == null)
            productionManager = FindAnyObjectByType<ProductionManager>();

        // Wyrejestruj �r�d�o przy zniszczeniu
        if (isRegistered && productionManager != null && incomeSource != null)
        {
            productionManager.UnregisterPassiveSource(incomeSource);
        }
    }

    /// IClickable implementation
    public void OnClick()
    {
        Debug.Log($"Clicked on workplace: {workplaceName}");
    }
    /// Zwraca dane do wy�wietlenia w UI po klikni�ciu
    public ClickableData GetClickableData()
    {
        var stats = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Doch�d", $"{CurrentIncome}/s" },
            { "Status", IsActive ? "Aktywne" : "Nieaktywne" },
            { "Pojemno��", $"{CurrentEmployees}/{Capacity}" }
        };
        return new ClickableData
        {
            Name = workplaceName,
            Type = workplaceType.ToString(),
            Description = description,
            Level = this.Level,
            Icon = null, // Mo�na przypisa� ikon� miejsca pracy tutaj
            Stats = stats
        };
    }
}
