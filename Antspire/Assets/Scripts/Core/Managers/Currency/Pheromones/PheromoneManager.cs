using UnityEngine;

/// <summary>
/// Zarządza feromonami - zasobem specjalnym (ResourceDef), który pełni rolę "paliwa"
/// dla kolonii. Feromony pozyskuje się przy extraction/production (są zwykłym zasobem,
/// np. trafiają do magazynów), ale nie są walutą ogólną - służą m.in. do odblokowywania
/// drzewka technologii, budowy szybszych dróg i innych zadań.
/// Saldo feromonów rośnie dzięki eventowi produkcji z ResourceManagera.
/// </summary>
public class PheromoneManager : MonoBehaviour
{
    [Header("Zasób feromonów")]
    [Tooltip("ResourceDef feromonów (np. plik Pheromones.asset) - zasób traktowany specjalnie.")]
    [SerializeField] private ResourceDef pheromoneResource;
    [SerializeField] private ResourceManager resourceManager;

    [Header("Konfiguracja Feromonów")]
    [SerializeField] private float startingPheromones = 50f;
    [SerializeField] private bool enableLogs = true;

    [Header("Statystyki (tylko do odczytu)")]
    [SerializeField] private float currentPheromones;
    [SerializeField] private float totalEarned;
    [SerializeField] private float totalSpent;
    [SerializeField] private float maxPheromones = 1000000f;

    // Eventy
    public System.Action<float> OnPheromonesChanged;
    public System.Action<float> OnPheromonesAdded;
    public System.Action<float> OnPheromonesSpent;

    // Properties
    public float CurrentPheromones => currentPheromones;
    public float TotalEarned => totalEarned;
    public float TotalSpent => totalSpent;

    /// <summary>ResourceDef feromonów - zasób służący jako "paliwo".</summary>
    public ResourceDef PheromoneResource => pheromoneResource;

    private void Awake()
    {
        currentPheromones = startingPheromones;
        Log("PheromoneManager zainicjalizowany. Startowe feromony: " + startingPheromones);
    }

    private void OnEnable()
    {
        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();

        if (resourceManager != null)
            resourceManager.OnResourceProduced += HandleResourceProduced;
        else
            LogWarning("Brak ResourceManager - feromony z produkcji nie będą naliczane.");
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnResourceProduced -= HandleResourceProduced;
    }

    /// <summary>
    /// Po wyprodukowaniu zasobu (extraction/production) - jeśli był to ResourceDef
    /// feromonów, dodaje je do "paliwa" kolonii.
    /// </summary>
    private void HandleResourceProduced(ResourceProducer producer, ItemStack stack)
    {
        if (stack == null || stack.Resource == null) return;
        if (pheromoneResource == null || stack.Resource != pheromoneResource) return;

        string source = producer != null ? producer.name : "Produkcja";
        AddPheromones(stack.Amount, source);
    }

    /// Dodaje feromony do "paliwa" gracza.
    public void AddPheromones(float amount, string source = "Unknown")
    {
        if (amount <= 0)
        {
            LogWarning($"Próba dodania nieprawidłowej ilości feromonów: {amount} ze źródła: {source}");
            return;
        }

        float added = Mathf.Min(amount, maxPheromones - currentPheromones);
        if (added <= 0)
        {
            Log($"Osiągnięto limit feromonów ({maxPheromones}). Źródło: {source}");
            return;
        }

        currentPheromones += added;
        totalEarned += added;

        OnPheromonesChanged?.Invoke(currentPheromones);
        OnPheromonesAdded?.Invoke(added);

        Log($"Dodano {added} feromonów ze źródła: {source}. Stan: {currentPheromones}");
    }

    /// Próbuje wydać feromony (odblokowanie techtree, budowa drogi itp.). Zwraca true, jeśli operacja się powiodła.
    public bool SpendPheromones(float amount, string reason = "Unknown")
    {
        if (amount <= 0)
        {
            LogWarning($"Próba wydania nieprawidłowej ilości feromonów: {amount} dla: {reason}");
            return false;
        }

        if (currentPheromones >= amount)
        {
            currentPheromones -= amount;
            totalSpent += amount;

            OnPheromonesChanged?.Invoke(currentPheromones);
            OnPheromonesSpent?.Invoke(amount);

            Log($"Wydano {amount} feromonów dla: {reason}. Stan: {currentPheromones}");
            return true;
        }

        Log($"Za mało feromonów! Potrzeba: {amount}, posiadasz: {currentPheromones} dla: {reason}");
        return false;
    }

    /// Sprawdza, czy gracza stać na dane zadanie (np. odblokowanie techtree).
    public bool CanAfford(float amount)
    {
        return currentPheromones >= amount;
    }

    // Metody pomocnicze do logowania
    private void Log(string message)
    {
        if (enableLogs)
            Debug.Log($"[PheromoneManager] {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[PheromoneManager] {message}");
    }
}
