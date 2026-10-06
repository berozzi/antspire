using System;
using UnityEngine;

/// <summary>
/// Behawior produkcji budynku - co interwał próbuje wyprodukować zasób
/// zgodnie z przypisanym SO: Extraction (bez inputu) albo Production (receptura z inputem).
/// Po każdym udanym wytworzeniu (zarówno po extraction, jak i po production)
/// uruchamia event <see cref="OnProduced"/> (dla tego budynku) oraz globalny
/// <see cref="OnResourceProduced"/>, z którego korzystają m.in. ResourceManager i ProductionManager.
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

    /// <summary>Dodaje wyprodukowany zasób do magazynu i odpala eventy produkcji.</summary>
    private bool AddOutput(ItemStack output)
    {
        if (output == null || output.Resource == null)
        {
            Debug.LogWarning($"{ProducerName} nie ma outputu do wyprodukowania.");
            return false;
        }

        storage.AddResource(output.Resource, output.Amount);

        // Event po wyprodukowaniu zasobu - najpierw dla tego budynku, potem globalny.
        // Logowanie zostało usunięte: event trafia do ResourceManager/UI, a tekst
        // co cykl na każdy budynek zalewałby konsolę i alokował stringi w buildzie.
        OnProduced?.Invoke(output);
        OnResourceProduced?.Invoke(this, output);
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
}
