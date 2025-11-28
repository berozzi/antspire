using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    [Header("Konfiguracja FeromonÛw")]
    [SerializeField] private float startingPheromones = 100f;
    [SerializeField] private bool enableLogs = true;

    [Header("Statystyki (tylko do odczytu)")]
    [SerializeField] private float currentPheromones;
    [SerializeField] private float totalEarned;
    [SerializeField] private float totalSpent;

    private List<PassiveIncomeSource> passiveSources = new List<PassiveIncomeSource>();
    private Coroutine passiveIncomeCoroutine;

    // Eventy
    public System.Action<float> OnPheromonesChanged;
    public System.Action<float> OnPheromonesAdded;
    public System.Action<float> OnPheromonesSpent;
    public System.Action<string> OnPheromonesSourceAdded;

    // Properties
    public float CurrentPheromones => currentPheromones;
    public float TotalEarned => totalEarned;
    public float TotalSpent => totalSpent;

    void Awake()
    {
        currentPheromones = startingPheromones;
        Log("PheromoneManager zainicjalizowany. Startowe feromony: " + startingPheromones);
    }

    void Start()
    {
        // Rozpocznij coroutine dla pasywnego dochodu
        passiveIncomeCoroutine = StartCoroutine(PassiveIncomeRoutine());
    }

    void OnDestroy()
    {
        // Zatrzymaj coroutine przy zniszczeniu
        if (passiveIncomeCoroutine != null)
            StopCoroutine(passiveIncomeCoroutine);
    }

    /// Dodaje feromony do zasobu gracza
    public void AddPheromones(float amount, string source = "Unknown")
    {
        if (amount <= 0)
        {
            LogWarning($"PrÛba dodania nieprawid≥owej iloúci feromonÛw: {amount} ze ürÛd≥a: {source}");
            return;
        }

        currentPheromones += amount;
        totalEarned += amount;

        OnPheromonesChanged?.Invoke(currentPheromones);
        OnPheromonesAdded?.Invoke(amount);

        Log($"Dodano {amount} feromonÛw ze ürÛd≥a: {source}. Stan: {currentPheromones}");
    }

    /// PrÛbuje wydaÊ feromony. Zwraca true jeúli operacja siÍ powiod≥a.
    public bool SpendPheromones(float amount, string reason = "Unknown")
    {
        if (amount <= 0)
        {
            LogWarning($"PrÛba wydania nieprawid≥owej iloúci feromonÛw: {amount} dla: {reason}");
            return false;
        }

        if (currentPheromones >= amount)
        {
            currentPheromones -= amount;
            totalSpent += amount;

            OnPheromonesChanged?.Invoke(currentPheromones);
            OnPheromonesSpent?.Invoke(amount);

            Log($"Wydano {amount} feromonÛw dla: {reason}. Stan: {currentPheromones}");
            return true;
        }
        else
        {
            Log($"Za ma≥o feromonÛw! Potrzeba: {amount}, Posiadasz: {currentPheromones} dla: {reason}");
            return false;
        }
    }

    /// Rejestruje ürÛd≥o pasywnego dochodu
    public void RegisterPassiveSource(PassiveIncomeSource source)
    {
        if (!passiveSources.Contains(source))
        {
            passiveSources.Add(source);
            OnPheromonesSourceAdded?.Invoke(source.SourceName);
            Log($"Zarejestrowano pasywne ürÛd≥o: {source.SourceName}");
        }
    }

    /// Wyrejestrowuje ürÛd≥o pasywnego dochodu
    public void UnregisterPassiveSource(PassiveIncomeSource source)
    {
        if (passiveSources.Contains(source))
        {
            passiveSources.Remove(source);
            Log($"Wyrejestrowano pasywne ürÛd≥o: {source.SourceName}");
        }
    }

    /// Sprawdza czy gracz ma wystarczajπco feromonÛw
    public bool CanAfford(float amount)
    {
        return currentPheromones >= amount;
    }

    // Coroutine dla pasywnego dochodu
    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Aktualizuj co sekundÍ

            if (passiveSources.Count > 0)
            {
                float totalPassiveIncome = 0f;
                List<string> activeSources = new List<string>();

                foreach (var source in passiveSources)
                {
                    if (source.IsActive)
                    {
                        float income = source.GetIncomePerSecond();
                        totalPassiveIncome += income;
                        activeSources.Add($"{source.SourceName}: {income}/s");
                    }
                }

                if (totalPassiveIncome > 0)
                {
                    AddPheromones(totalPassiveIncome, "Pasywny dochÛd");
                    // debug logs sπ zakomentowane, aby uniknπÊ nadmiernego logowania
                    //if (enableLogs)
                    //{
                    //    Log($"Pasywny dochÛd: {totalPassiveIncome}/s èrÛd≥a: {string.Join(", ", activeSources)}");
                    //}
                }
            }
        }
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
