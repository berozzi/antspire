using UnityEngine;

/// Odpowiada wyłącznie za walutę gracza - feromony (dodawanie, wydawanie,
/// sprawdzanie dostępności). Pasywny dochód przejął ProductionManager.
public class PheromoneManager : MonoBehaviour
{
    [Header("Konfiguracja Feromonów")]
    [SerializeField] private float startingPheromones = 50f;
    [SerializeField] private bool enableLogs = true;

    [Header("Statystyki (tylko do odczytu)")]
    [SerializeField] private float currentPheromones;
    [SerializeField] private float totalEarned;
    [SerializeField] private float totalSpent;
    float maxPheromones = 1000000f;

    // Eventy
    public System.Action<float> OnPheromonesChanged;
    public System.Action<float> OnPheromonesAdded;
    public System.Action<float> OnPheromonesSpent;

    // Properties
    public float CurrentPheromones => currentPheromones;
    public float TotalEarned => totalEarned;
    public float TotalSpent => totalSpent;

    void Awake()
    {
        currentPheromones = startingPheromones;
        Log("PheromoneManager zainicjalizowany. Startowe feromony: " + startingPheromones);
    }

    /// Dodaje feromony do zasobu gracza.
    public void AddPheromones(float amount, string source = "Unknown")
    {
        if (amount <= 0 || currentPheromones + amount > maxPheromones)
        {
            LogWarning($"Pr�ba dodania nieprawid�owej ilo�ci feromon�w: {amount} ze �r�d�a: {source}");
            return;
        }

        currentPheromones += amount;
        totalEarned += amount;

        OnPheromonesChanged?.Invoke(currentPheromones);
        OnPheromonesAdded?.Invoke(amount);
    }

    /// Próbuje wydać feromony. Zwraca true, jeśli operacja się powiodła.
    public bool SpendPheromones(float amount, string reason = "Unknown")
    {
        if (amount <= 0)
        {
            LogWarning($"Pr�ba wydania nieprawid�owej ilo�ci feromon�w: {amount} dla: {reason}");
            return false;
        }

        if (currentPheromones >= amount)
        {
            currentPheromones -= amount;
            totalSpent += amount;

            OnPheromonesChanged?.Invoke(currentPheromones);
            OnPheromonesSpent?.Invoke(amount);

            Log($"Wydano {amount} feromon�w dla: {reason}. Stan: {currentPheromones}");
            return true;
        }
        else
        {
            Log($"Za ma�o feromon�w! Potrzeba: {amount}, Posiadasz: {currentPheromones} dla: {reason}");
            return false;
        }
    }

    /// Sprawdza, czy gracz ma wystarczająco feromonów.
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