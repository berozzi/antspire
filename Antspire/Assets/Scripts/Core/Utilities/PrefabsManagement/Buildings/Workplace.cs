using System.Collections.Generic;
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

    [Header("Produkcja i magazyn (puste = szukane w hierarchii)")]
    [SerializeField] private ResourceProducer producer;

    [Header("Taski wykonywane w tym miejscu")]
    [SerializeField] private List<WorkTaskDefinition> tasks = new List<WorkTaskDefinition>();

    // Rejestracja w puli miejsc pracy - pule nie muszą przeszukiwać sceny.
    private WorkplacePools registeredPools;

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

    void OnEnable()
    {
        // Jedno wyszukanie na cykl życia miejsca pracy - potem pule wołają nas same.
        if (registeredPools == null)
            registeredPools = FindAnyObjectByType<WorkplacePools>();

        if (registeredPools != null)
            registeredPools.Register(this);
    }

    void OnDisable()
    {
        if (registeredPools != null)
            registeredPools.Unregister(this);
    }

    /// <summary>Zapina dwustronny link pula ↔ miejsce pracy (wołane z WorkplacePools.Register).</summary>
    internal void AttachPool(WorkplacePools pools)
    {
        registeredPools = pools;
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

    public bool HasAvailableCapacity()
    {
        return AvailableSpots > 0;
    }

    /// <summary>
    /// Budynek produkcyjny tego miejsca pracy - źródło towaru dla mrówki.
    /// Pole można wypełnić w Inspektorze; puste jest szukane w hierarchii obiektu.
    /// </summary>
    public ResourceProducer Producer
    {
        get
        {
            if (producer == null) producer = GetComponent<ResourceProducer>();
            if (producer == null) producer = GetComponentInParent<ResourceProducer>();
            return producer;
        }
    }

    /// <summary>Magazyn miejsca pracy (null, gdy miejsce nie ma producenta zasobów).</summary>
    public ResourceStorage Storage
    {
        get
        {
            ResourceProducer current = Producer;
            return current != null ? current.Storage : null;
        }
    }

    /// <summary>Procent obsadzenia miejsca pracy (0-100) - na jego podstawie pule sortują miejsca pracy.</summary>
    public float OccupationPercent => Capacity <= 0 ? 100f : 100f * CurrentEmployees / Capacity;

    /// <summary>Wolne etaty w tym miejscu pracy.</summary>
    public int AvailableSpots => Mathf.Max(0, Capacity - CurrentEmployees);

    /// <summary>Próba zajęcia jednego etatu. Zwraca false, gdy miejsce jest już pełne.</summary>
    public bool TryOccupy()
    {
        if (AvailableSpots <= 0) return false;

        currentEmployees += 1;
        NotifyOccupationChanged();
        return true;
    }

    /// <summary>Zwolnienie jednego etatu zajętego przez mrówkę (po ukończeniu taska albo przerwaniu go).</summary>
    public void Release()
    {
        currentEmployees = Mathf.Max(0, currentEmployees - 1);
        NotifyOccupationChanged();
    }

    /// <summary>Zmiana obsadzenia przestawia to miejsce do właściwej puli od razu, bez czekania na odświeżenie.</summary>
    private void NotifyOccupationChanged()
    {
        if (registeredPools != null)
            registeredPools.Reclassify(this);
    }

    /// <summary>Losowo dobiera task do wykonania. Zwraca null, gdy miejsce nie ma zdefiniowanych tasków.</summary>
    public WorkTaskDefinition PickRandomTask()
    {
        if (tasks == null || tasks.Count == 0) return null;
        return tasks[Random.Range(0, tasks.Count)];
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
