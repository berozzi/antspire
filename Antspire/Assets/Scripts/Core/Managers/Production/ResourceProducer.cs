using System;
using UnityEngine;

/// <summary>
/// Behawior produkcji budynku - co interwa� pr�buje wyprodukowa� zas�b
/// zgodnie z przypisanym SO: Extraction (bez inputu) albo Production (receptura z inputem).
/// </summary>
public class ResourceProducer : MonoBehaviour
{
    [Header("Definicje")]
    [SerializeField] private Extraction extraction;
    [SerializeField] private Production production;

    [Header("Rytm produkcji")]
    [SerializeField] private float intervalSeconds = 1f;

    [Header("Magazyn")]
    [SerializeField] private ResourceStorage storage;

    private float timer;

    public event Action<ItemStack> OnProduced;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < intervalSeconds) return;

        timer = 0f;
        TryProduce();
    }

    /// <summary>Pr�buje wykona� jeden cykl produkcji. Zwraca true, gdy si� uda�o.</summary>
    public bool TryProduce()
    {
        if (storage == null) return false;

        if (extraction != null && extraction.Output != null)
        {
            AddOutput(extraction.Output);
            return true;
        }

        if (production != null)
        {
            ProduceFromRecipe();
            return true;
        }

        return false;
    }

    private void ProduceFromRecipe()
    {
        ItemStack[] input = production.input;
        if (input == null)
        {
            Debug.LogWarning($"{ProducerName} potrzebuje surowców do produkcji.");
            return;
        }

        // Sprawdz czy starczy surowcw.
        foreach (ItemStack stack in input)
        {
            if (stack == null || !storage.CanWithdraw(stack.Resource, stack.Amount))
                return;
        }

        // Zuzyj inputy.
        foreach (ItemStack stack in input)
            storage.TryWithdraw(stack.Resource, stack.Amount);

        // Dodaj output.
        if (production.output != null)
            AddOutput(production.output);
    }

    private void AddOutput(ItemStack output)
    {
        if (output == null || output.Resource == null)
        {
            Debug.LogWarning($"{ProducerName} nie ma outputu do wyprodukowania.");
            return;
        }

        storage.AddResource(output.Resource, output.Amount);
        Debug.Log($"{ProducerName} wyprodukowal: {output.Amount}x {output.Resource.ResourceName}");
        OnProduced?.Invoke(output);
    }

    private string ProducerName
    {
        get
        {
            if (extraction != null && !string.IsNullOrEmpty(extraction.BuildingName))
                return extraction.BuildingName;

            if (production != null && !string.IsNullOrEmpty(production.buildingName))
                return production.buildingName;

            return gameObject.name;
        }
    }
}