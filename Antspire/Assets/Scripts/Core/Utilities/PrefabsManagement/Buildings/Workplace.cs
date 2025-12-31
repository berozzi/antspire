using UnityEngine;

public class Workplace : MonoBehaviour, ISaveable, IClickable
{
    [Header("Dochód Pasywny")]
    [SerializeField] private string workplaceName = "Miejsce Pracy";
    [SerializeField] private string description = "Opis miejsca pracy.";
    [SerializeField] private float baseIncomePerSecond = 1f;
    [SerializeField] private bool generatesIncome = true; // set to false if this workplace does not generate pheromone income
    [SerializeField] private WorkplaceType workplaceType;
    [SerializeField] private int capacity = 1;
    [SerializeField] private int currentEmployees = 0;
    [SerializeField] private int level = 1;

    [Header("Referencje")]
    [SerializeField] private PheromoneManager pheromoneManager;

    private PassiveIncomeSource incomeSource;
    private bool isRegistered = false;

    // Properties dla AI
    public string WorkplaceName => workplaceName;
    public string Description => description;
    public bool IsActive => incomeSource?.IsActive ?? false;
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

        // Utwórz Ÿród³o dochodu
        incomeSource = new PassiveIncomeSource(workplaceName, baseIncomePerSecond);

        // Zarejestruj siê w managerze
        if (pheromoneManager != null)
        {
            pheromoneManager.RegisterPassiveSource(incomeSource);
            isRegistered = true;
        }
        else
        {
            Debug.LogWarning($"PheromoneManager nie przypisany do Workplace: {workplaceName}");
        }
    }

    /// Ulepsza dochód z tego miejsca pracy
    public void UpgradeIncome(float upgradeAmount)
    {
        if (incomeSource != null)
        {
            incomeSource.UpgradeBaseIncome(upgradeAmount);
            Debug.Log($"Ulepszono {workplaceName}. Nowy dochód: {incomeSource.GetIncomePerSecond()}/s");
        }
    }
   
    /// Ustawia now¹ bazow¹ wartoœæ dochodu
    public void SetBaseIncome(float newIncome)
    {
        if (incomeSource != null)
        {
            // Mo¿emy dodaæ logikê obliczania ró¿nicy i aktualizacji
            baseIncomePerSecond = newIncome;
            // Tutaj potrzebowalibyœmy metody do aktualizacji w PassiveIncomeSource
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
    /// Aktywuje/dezaktywuje generowanie dochodu
    public void SetIncomeActive(bool active)
    {
        if (incomeSource != null)
        {
            incomeSource.SetActive(active);
            Debug.Log($"{workplaceName} - generowanie dochodu: {(active ? "AKTYWNE" : "WY£¥CZONE")}");
        }
    }
    void OnDestroy()
    {
        SaveManager.Unregister(this);
        // Wyrejestruj Ÿród³o przy zniszczeniu
        if (isRegistered && pheromoneManager != null && incomeSource != null)
        {
            pheromoneManager.UnregisterPassiveSource(incomeSource);
        }
    }

    /// IClickable implementation
    public void OnClick()
    {
        Debug.Log($"Clicked on workplace: {workplaceName}");
    }
    /// Zwraca dane do wyœwietlenia w UI po klikniêciu
    public ClickableData GetClickableData()
    {
        var stats = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Dochód", $"{CurrentIncome}/s" },
            { "Status", IsActive ? "Aktywne" : "Nieaktywne" },
            { "Poziom", level.ToString() },
            { "Pojemnoœæ", $"{CurrentEmployees}/{Capacity}" }
        };
        return new ClickableData
        {
            Name = workplaceName,
            Type = workplaceType.ToString(),
            Description = description,
            Icon = null, // Mo¿na przypisaæ ikonê miejsca pracy tutaj
            Stats = stats
        };
    }
}
