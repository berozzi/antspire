using System;
using System.Collections.Generic;
using UnityEngine;

/// Zarządza zasobami kolonii (drewno, kamień, jedzenie itd.).
public class ResourceManager : MonoBehaviour
{
    [Header("Startowe zasoby")]
    [SerializeField] private int startingWood = 0;
    [SerializeField] private int startingStone = 0;

    private readonly Dictionary<ResourceType, int> amounts = new Dictionary<ResourceType, int>();

    public event Action<ResourceType, int> OnResourcesChanged;

    private void Awake()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            amounts[type] = 0;

        amounts[ResourceType.Wood] = startingWood;
        amounts[ResourceType.Stone] = startingStone;

        // Zasiej zasoby z zapisu, jeśli istnieją.
        GameSave gameSave = GameSave.Instance;
        if (gameSave != null && gameSave.resources != null)
        {
            foreach (ResourceData data in gameSave.resources)
            {
                if (data != null)
                    amounts[data.type] = data.value;
            }
        }
    }

    public int GetAmount(ResourceType type)
    {
        return amounts.TryGetValue(type, out int value) ? value : 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        amounts[type] = GetAmount(type) + amount;
        OnResourcesChanged?.Invoke(type, GetAmount(type));
    }

    public bool CanAfford(ResourceType type, int amount)
    {
        return GetAmount(type) >= amount;
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (amount <= 0 || !CanAfford(type, amount))
            return false;

        amounts[type] = GetAmount(type) - amount;
        OnResourcesChanged?.Invoke(type, GetAmount(type));
        return true;
    }

    public void SetResource(ResourceType type, int amount)
    {
        amounts[type] = amount;
        OnResourcesChanged?.Invoke(type, amount);
    }
}