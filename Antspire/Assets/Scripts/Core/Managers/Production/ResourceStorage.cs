using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lokalny magazyn budynku - przechowuje ilości zasobów (ResourceDef) produkowanych
/// lub zużywanych przez miejsce produkcji. Mrówki oddają/zabierają surowce przez to API.
/// Magazyn sam rejestruje się w ResourceManagerze, żeby ten mógł zbierać dane ze sceny.
/// </summary>
public class ResourceStorage : MonoBehaviour
{
    private readonly Dictionary<ResourceDef, int> amounts = new Dictionary<ResourceDef, int>();

    private ResourceManager registeredManager;

    public event Action<ResourceDef, int> OnChanged;

    /// <summary>Zasoby aktualnie znajdujące się w magazynie.</summary>
    public IEnumerable<ResourceDef> StoredResources => amounts.Keys;

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
