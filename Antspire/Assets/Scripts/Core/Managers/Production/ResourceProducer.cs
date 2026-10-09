using System;
using UnityEngine;

/// <summary>
/// Behawior produkcji budynku - co interwał próbuje wyprodukować zasób
/// zgodnie z przypisanym SO: Extraction (bez inputu) albo Production (receptura z inputem).
/// Wytworzony towar trafia do <see cref="Storage"/>, skąd mrówki zabierają go do transportu
/// (patrz <see cref="AntWorkerAI"/>), a budynki zużywają go jako input receptury.
/// Po każdym udanym wytworzeniu (zarówno po extraction, jak i po production)
/// uruchamia event <see cref="OnProduced"/> (dla tego budynku) oraz globalny
/// <see cref="OnResourceProduced"/>, z którego korzystają m.in. ResourceManager i ProductionManager.
/// Do walidacji trasy służy <see cref="OutputResource"/> (co budynek daje)
/// oraz <see cref="AcceptsInput"/> (czego budynek potrzebuje).
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

    [Header("Ikony i inne wizualizacje")]
    [SerializeField] private GameObject outputIconPrefab;

    private float timer;
    private int productionCount = 0;

    public ResourceStorage Storage => storage;

    /// <summary>Zasób wychodzący z budynku: output extractionu albo output receptury produkcji.</summary>
    public ResourceDef OutputResource
    {
        get
        {
            if (extraction != null) return extraction.OutputResource;
            if (production != null && production.output != null) return production.output.Resource;
            return null;
        }
    }

    /// <summary>
    /// Czy receptura tego budynku zużywa podany zasób jako input - na tym opiera się
    /// walidacja trasy: output budynku startowego musi być inputem budynku docelowego.
    /// </summary>
    public bool AcceptsInput(ResourceDef resource)
    {
        if (resource == null || production == null || production.input == null) return false;

        foreach (ItemStack stack in production.input)
        {
            if (stack != null && stack.Resource == resource) return true;
        }

        return false;
    }

    private void Awake()
    {
        // Fallback dla prefabów z niewypełnionym polem "storage" (np. quarry) -
        // magazyn zwykle stoi na tym samym obiekcie co producent.
        if (storage == null)
            storage = GetComponent<ResourceStorage>();
    }

    /// <summary>Event tego budynku - informuje o wyprodukowanym stosie zasobu.</summary>
    public event Action<ItemStack> OnProduced;
    /// <summary>
    /// Globalny event wywoływany po wyprodukowaniu zasobu (extraction/production).
    /// Argumenty: budynek, który wyprodukował, oraz wyprodukowany stos.
    /// </summary>
    public static event Action<ResourceProducer, ItemStack> OnResourceProduced;

    // Reset statycznego eventu przy starcie play mode (w razie wyłączonego domain reloadu).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticEvents()
    {
        OnResourceProduced = null;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < intervalSeconds) return;

        timer = 0f;
        TryProduce();
    }

    /// <summary>Próbuje wykonać jeden cykl produkcji. Zwraca true, gdy udało się wytworzyć zasób.</summary>
    public bool TryProduce()
    {
        if (storage == null) return false;

        if (extraction != null && extraction.Output != null)
            return AddOutput(extraction.Output);

        if (production != null)
            return ProduceFromRecipe();

        return false;
    }

    private bool ProduceFromRecipe()
    {
        ItemStack[] input = production.input;
        if (input == null)
        {
            Debug.LogWarning($"{ProducerName} potrzebuje surowców do produkcji.");
            return false;
        }

        // Sprawdź, czy starcza surowców.
        foreach (ItemStack stack in input)
        {
            if (stack == null || !storage.CanWithdraw(stack.Resource, stack.Amount))
                return false;
        }

        // Zużyj inputy.
        foreach (ItemStack stack in input)
            storage.TryWithdraw(stack.Resource, stack.Amount);

        // Dodaj output.
        if (production.output == null)
        {
            Debug.LogWarning($"{ProducerName} nie ma outputu do wyprodukowania.");
            return false;
        }

        return AddOutput(production.output);
    }

    /// <summary>Dodaje wyprodukowany zasób do magazynu i odpala eventy produkcji. Blokuje jeśli pełny</summary>
    private bool AddOutput(ItemStack output)
    {
        if (output == null || output.Resource == null)
        {
            Debug.LogWarning($"{ProducerName} nie ma outputu do wyprodukowania.");
            return false;
        }

        if (storage.isFull)
        {
            Debug.LogWarning($"{ProducerName} nie może wyprodukować {output.Resource.name}, magazyn pełny.");
            return false;
        }
        // Dodaj output do magazynu.
        storage.AddResource(output.Resource, output.Amount);

        // Event po wyprodukowaniu zasobu - najpierw dla tego budynku, potem globalny.
        OnProduced?.Invoke(output);
        OnResourceProduced?.Invoke(this, output);
        productionCount++;
        if (productionCount == 1)
        {
            ShowOutputIcon(output.Amount);
        }
        return true;
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
    // Po wyprodukowaniu, pojawienie ikony outputu (jeśli przypisano prefab) - np. dla wizualizacji w UI.
    private void ShowOutputIcon(int amount)
    {
        //string amountText = amount.ToString();
        if (outputIconPrefab != null)
        {
            Instantiate(outputIconPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        }
    }
}
