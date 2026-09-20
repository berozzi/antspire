using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Odpowiada za produkcję kolonii: rejestruje źródła pasywnego dochodu
/// (np. miejsca pracy) i co sekundę zamienia ich produkcję na feromony.
public class ProductionManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PheromoneManager pheromoneManager;

    private readonly List<PassiveIncomeSource> passiveSources = new List<PassiveIncomeSource>();
    private Coroutine passiveIncomeCoroutine;

    public event System.Action<string> OnPheromonesSourceAdded;

    private void Start()
    {
        if (pheromoneManager == null)
            pheromoneManager = FindAnyObjectByType<PheromoneManager>();

        if (pheromoneManager != null)
            passiveIncomeCoroutine = StartCoroutine(PassiveIncomeRoutine());
    }

    private void OnDestroy()
    {
        if (passiveIncomeCoroutine != null)
            StopCoroutine(passiveIncomeCoroutine);
    }

    /// Rejestruje źródło pasywnego dochodu.
    public void RegisterPassiveSource(PassiveIncomeSource source)
    {
        if (!passiveSources.Contains(source))
        {
            passiveSources.Add(source);
            OnPheromonesSourceAdded?.Invoke(source.SourceName);
        }
    }

    /// Wyrejestrowuje źródło pasywnego dochodu.
    public void UnregisterPassiveSource(PassiveIncomeSource source)
    {
        if (passiveSources.Contains(source))
            passiveSources.Remove(source);
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // aktualizuj co sekundę

            if (passiveSources.Count == 0)
                continue;

            float totalPassiveIncome = 0f;

            foreach (var source in passiveSources)
            {
                if (source.IsActive)
                    totalPassiveIncome += source.GetIncomePerSecond();
            }

            if (totalPassiveIncome > 0 && pheromoneManager != null)
                pheromoneManager.AddPheromones(totalPassiveIncome, "Pasywny dochód");
        }
    }
}