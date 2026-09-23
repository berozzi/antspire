using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lokalny magazyn budynku - przechowuje ilo�ci zasob�w (ResourceDef) produkowanych
/// lub zu�ywanych przez miejsce produkcji. Mr�wki oddaj�/zabieraj� surowce przez to API.
/// </summary>
public class ResourceStorage : MonoBehaviour
{
    private readonly Dictionary<ResourceDef, int> amounts = new Dictionary<ResourceDef, int>();

    public event Action<ResourceDef, int> OnChanged;

    public int GetAmount(ResourceDef resource)
    {
        if (resource == null) return 0;
        return amounts.TryGetValue(resource, out int value) ? value : 0;
    }

    /// <summary>Oddaje zas�b do magazynu (np. mr�wka przynosz�ca surowiec).</summary>
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

    /// <summary>Pobiera zas�b z magazynu (np. produkcja zu�ywaj�ca input).</summary>
    public bool TryWithdraw(ResourceDef resource, int amount)
    {
        if (!CanWithdraw(resource, amount)) return false;

        amounts[resource] = GetAmount(resource) - amount;
        OnChanged?.Invoke(resource, GetAmount(resource));
        return true;
    }
}