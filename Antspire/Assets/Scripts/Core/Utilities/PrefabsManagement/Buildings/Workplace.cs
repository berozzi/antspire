using UnityEngine;

public class Workplace : MonoBehaviour, ISaveable
{
    [Header("Dochód Pasywny")]
    [SerializeField] private string workplaceName = "Miejsce Pracy";
    [SerializeField] private float baseIncomePerSecond = 1f;
    [SerializeField] private bool generatesIncome = true;

    [Header("Referencje")]
    [SerializeField] private PheromoneManager pheromoneManager;

    private PassiveIncomeSource incomeSource;
    private bool isRegistered = false;

    // Properties dla AI
    public string WorkplaceName => workplaceName;
    public bool IsActive => incomeSource?.IsActive ?? false;
    public float CurrentIncome => incomeSource?.GetIncomePerSecond() ?? 0f;

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

    /// Aktywuje/dezaktywuje generowanie dochodu
    public void SetIncomeActive(bool active)
    {
        if (incomeSource != null)
        {
            incomeSource.SetActive(active);
            Debug.Log($"{workplaceName} - generowanie dochodu: {(active ? "AKTYWNE" : "WY£¥CZONE")}");
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

    /// Zwraca informacje o miejscu pracy dla UI
    public string GetWorkplaceInfo()
    {
        if (!generatesIncome) return $"{workplaceName} (brak dochodu)";

        return $"{workplaceName}\nDochód: {CurrentIncome}/s\nStatus: {(IsActive ? "Aktywne" : "Nieaktywne")}";
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

    public object GetSaveData()
    {
        return new WorkplaceData
        {
            workplaceName = this.workplaceName,
            baseIncomePerSecond = this.baseIncomePerSecond,
            generatesIncome = this.generatesIncome,
            isActive = this.IsActive
        };
    }
}
