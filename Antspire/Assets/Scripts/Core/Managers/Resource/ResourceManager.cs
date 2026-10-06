using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zarządza zasobami kolonii (drewno, kamień, jedzenie, feromony itd.).
/// Zbiera dane ze sceny - agreguje stany wszystkich <see cref="ResourceStorage"/>
/// i informuje (eventem <see cref="OnResourceTotalChanged"/>), gdy któregoś zasobu
/// jest więcej lub mniej. Dodatkowo przekazuje zdarzenie produkcji
/// (<see cref="OnResourceProduced"/>) wywoływane przez <see cref="ResourceProducer"/>
/// po extraction/production.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Konfiguracja")]
    [SerializeField] private bool enableLogs = true;

    /// <summary>Sumaryczna ilość każdego zasobu we wszystkich magazynach kolonii.</summary>
    private readonly Dictionary<ResourceDef, int> totals = new Dictionary<ResourceDef, int>();

    /// <summary>Magazyny, z których ResourceManager czyta dane.</summary>
    private readonly List<ResourceStorage> storages = new List<ResourceStorage>();

    /// <summary>
    /// Zmiana sumarycznej ilości zasobu w kolonii - argumenty: zasób oraz nowa całkowita ilość.
    /// Gdy wartość rośnie, oznacza, że "jest więcej jakiegoś zasobu".
    /// </summary>
    public event Action<ResourceDef, int> OnResourceTotalChanged;

    /// <summary>
    /// Wywoływany po wyprodukowaniu zasobu przez dowolny <see cref="ResourceProducer"/>
    /// (extraction albo production). Argumenty: budynek oraz wyprodukowany stos.
    /// </summary>
    public event Action<ResourceProducer, ItemStack> OnResourceProduced;

    /// <summary>Sumaryczne ilości wszystkich znanych zasobów (tylko do odczytu).</summary>
    public IReadOnlyDictionary<ResourceDef, int> Totals => totals;

    private void OnEnable()
    {
        ResourceProducer.OnResourceProduced += HandleResourceProduced;
    }

    private void OnDisable()
    {
        ResourceProducer.OnResourceProduced -= HandleResourceProduced;
    }

    private void Start()
    {
        RefreshFromScene();
    }

    /// <summary>Od nowa znajduje wszystkie magazyny w scenie i przelicza sumy.</summary>
    public void RefreshFromScene()
    {
        foreach (ResourceStorage storage in FindObjectsByType<ResourceStorage>())
            RegisterStorage(storage);
    }

    /// <summary>Dopisuje magazyn do zbieranych danych (idempotentne).</summary>
    public void RegisterStorage(ResourceStorage storage)
    {
        if (storage == null || storages.Contains(storage)) return;

        storages.Add(storage);
        storage.OnChanged += HandleStorageChanged;

        // Przelicz zasoby, które magazyn już zawierał przed rejestracją.
        foreach (ResourceDef resource in storage.StoredResources)
            RecalculateTotal(resource);

        Log($"Zarejestrowano magazyn: {storage.name}");
    }

    /// <summary>Usuwa magazyn ze zbieranych danych.</summary>
    public void UnregisterStorage(ResourceStorage storage)
    {
        if (storage == null) return;

        if (storages.Remove(storage))
        {
            storage.OnChanged -= HandleStorageChanged;

            foreach (ResourceDef resource in storage.StoredResources)
                RecalculateTotal(resource);
        }
    }

    /// <summary>Sumaryczna ilość zasobu w całej kolonii.</summary>
    public int GetTotal(ResourceDef resource)
    {
        if (resource == null) return 0;
        return totals.TryGetValue(resource, out int value) ? value : 0;
    }

    /// <summary>Sumaryczna ilość zasobu szukając po nazwie wyświetlanej albo po nazwie pliku .asset.</summary>
    public int GetTotal(string resourceName)
    {
        ResourceDef resource = FindResource(resourceName);
        return GetTotal(resource);
    }

    /// <summary>Znajduje definicję zasobu po nazwie wyświetlanej albo po nazwie pliku .asset.</summary>
    public ResourceDef FindResource(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName)) return null;

        foreach (KeyValuePair<ResourceDef, int> entry in totals)
        {
            if (entry.Key == null) continue;

            if (string.Equals(entry.Key.ResourceName, resourceName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entry.Key.Id, resourceName, StringComparison.OrdinalIgnoreCase))
                return entry.Key;
        }

        return null;
    }

    private void HandleStorageChanged(ResourceDef resource, int storageAmount)
    {
        RecalculateTotal(resource);
    }

    private void HandleResourceProduced(ResourceProducer producer, ItemStack stack)
    {
        if (stack == null || stack.Resource == null) return;

        Log($"Produkcja ({producer?.name}): {stack.Amount}x {stack.Resource.ResourceName}");
        OnResourceProduced?.Invoke(producer, stack);
    }

    /// <summary>Przelicza sumę zasobu we wszystkich magazynach i odpala event, jeśli się zmieniła.</summary>
    private void RecalculateTotal(ResourceDef resource)
    {
        if (resource == null) return;

        int total = 0;
        for (int i = storages.Count - 1; i >= 0; i--)
        {
            if (storages[i] == null)
            {
                storages.RemoveAt(i);
                continue;
            }

            total += storages[i].GetAmount(resource);
        }

        if (totals.TryGetValue(resource, out int previous) && previous == total) return;

        totals[resource] = total;
        Log($"Zasób {resource.ResourceName}: {previous} -> {total}");
        OnResourceTotalChanged?.Invoke(resource, total);
    }

    private void Log(string message)
    {
        if (enableLogs)
            Debug.Log($"[ResourceManager] {message}");
    }
}
