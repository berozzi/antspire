using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rejestruje wszystkie <see cref="ResourceProducer"/> w scenie i udostępnia
/// zbiorcze zdarzenie produkcji (<see cref="OnProduced"/>), powiązane z eventem
/// <see cref="ResourceProducer.OnResourceProduced"/> uruchamianym po extraction/production.
/// Dawniej zarządzał pasywnym dochodem feromonów - teraz feromony to zwykły zasób
/// (ResourceDef) zdobywany podczas produkcji i traktowany przez PheromoneManager specjalnie.
/// </summary>
public class ProductionManager : MonoBehaviour
{
    [Header("Konfiguracja")]
    [SerializeField] private bool enableLogs = true;

    private readonly List<ResourceProducer> producers = new List<ResourceProducer>();

    /// <summary>Zbiorczy event wywoływany po wyprodukowaniu zasobu przez dowolny budynek.</summary>
    public event Action<ResourceProducer, ItemStack> OnProduced;

    /// <summary>Wszystkie znane budynki produkcyjne (tylko do odczytu).</summary>
    public IReadOnlyList<ResourceProducer> Producers => producers;

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

    /// <summary>Od nowa znajduje wszystkie budynki produkcyjne w scenie.</summary>
    public void RefreshFromScene()
    {
        foreach (ResourceProducer producer in FindObjectsByType<ResourceProducer>())
            RegisterProducer(producer);
    }

    /// <summary>Dopisuje budynek produkcyjny do rejestru (idempotentne).</summary>
    public void RegisterProducer(ResourceProducer producer)
    {
        if (producer == null || producers.Contains(producer)) return;

        producers.Add(producer);
        if (enableLogs) Debug.Log($"[ProductionManager] Zarejestrowano producenta: {producer.name}");
    }

    /// <summary>Usuwa budynek produkcyjny z rejestru.</summary>
    public void UnregisterProducer(ResourceProducer producer)
    {
        if (producer != null)
            producers.Remove(producer);
    }

    private void HandleResourceProduced(ResourceProducer producer, ItemStack stack)
    {
        if (producer != null && !producers.Contains(producer))
            producers.Add(producer);

        OnProduced?.Invoke(producer, stack);
    }
}
