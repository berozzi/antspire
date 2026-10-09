using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lokalny magazyn budynku - przechowuje ilości zasobów (ResourceDef) produkowanych
/// lub zużywanych przez miejsce produkcji. Mrówki oddają/zabierają surowce przez to API:
/// <see cref="ResourceProducer"/> wpisuje tu output, a <see cref="AntWorkerAI"/> pobiera
/// go do transportu (stąd realne zużycie zasobów). Magazyn sam rejestruje się
/// w ResourceManagerze, żeby ten mógł zbierać dane ze sceny.
/// </summary>
public class ResourceStorage : MonoBehaviour
{
    private readonly Dictionary<ResourceDef, int> amounts = new ();

    private ResourceManager registeredManager;

    public event Action<ResourceDef, int> OnChanged;

    /// <summary>Zasoby aktualnie znajdujące się w magazynie.</summary>
    public IEnumerable<ResourceDef> StoredResources => amounts.Keys;

    /// <summary>Maksymalna pojemność magazynu (suma wszystkich zasobów).</summary>
    [SerializeField] private int maxCapacity = 20;

    /// <summary>Maksymalna pojemność magazynu - do UI i przeliczania pełności.</summary>
    public int MaxCapacity => maxCapacity;

    /// <summary>Aktualna łączna liczba zasobów w magazynie.</summary>
    public int CurrentCount
    {
        get
        {
            int total = 0;
            foreach (int amount in amounts.Values) total += amount;
            return total;
        }
    }

    /// <summary>Wypełnienie magazynu w zakresie 0-1 - próg kontroli mrówki (domyślnie 0.5).</summary>
    public float FillRatio => maxCapacity <= 0 ? 1f : (float)CurrentCount / maxCapacity;

    /// <summary>Czy magazyn nie przyjmie już więcej zasobów.</summary>
    public bool isFull => CurrentCount >= maxCapacity;

    private void OnEnable()
    {
        if (registeredManager == null)
            registeredManager = FindAnyObjectByType<ResourceManager>();

        if (registeredManager != null)
            registeredManager.RegisterStorage(this);
    }

    private void OnDisable()
    {
        if (registeredManager != null)
            registeredManager.UnregisterStorage(this);

        registeredManager = null;
    }

    public int GetAmount(ResourceDef resource)
    {
        if (resource == null) return 0;
        return amounts.TryGetValue(resource, out int value) ? value : 0;
    }

    /// <summary>Oddaje zasób do magazynu (np. mrówka przynosząca surowiec).</summary>
    public void AddResource(ResourceDef resource, int amount)
    {
        if (resource == null || amount <= 0) return;

        amounts[resource] = GetAmount(resource) + amount;
        OnChanged?.Invoke(resource, GetAmount(resource));
    }

    /// <summary>Czy da się odłożyć tyle zasobu bez przekroczenia pojemności - warunek rozładunku.</summary>
    public bool CanAdd(ResourceDef resource, int amount)
    {
        return resource != null && amount > 0 && CurrentCount + amount <= maxCapacity;
    }

    public bool CanWithdraw(ResourceDef resource, int amount)
    {
        return resource != null && amount > 0 && GetAmount(resource) >= amount;
    }

    /// <summary>Pobiera zasób z magazynu (np. produkcja zużywająca input).</summary>
    public bool TryWithdraw(ResourceDef resource, int amount)
    {
        if (!CanWithdraw(resource, amount)) return false;

        amounts[resource] = GetAmount(resource) - amount;
        OnChanged?.Invoke(resource, GetAmount(resource));
        return true;
    }
}
