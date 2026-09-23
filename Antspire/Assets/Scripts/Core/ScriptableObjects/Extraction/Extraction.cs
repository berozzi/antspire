using UnityEngine;

/// <summary>
/// Budynek wydobywczy - definiuje nazwę budynku oraz output (co produkuje).
/// </summary>
[CreateAssetMenu(fileName = "Extraction", menuName = "ScriptableObjects/Production/Extraction")]
public class Extraction : ScriptableObject
{
    [Header("Informacje o budynku")]
    [SerializeField] private string buildingName;

    [Header("Produkcja")]
    [SerializeField] private ItemStack output;

    /// <summary>Nazwa budynku wydobywczego.</summary>
    public string BuildingName => buildingName;

    /// <summary>Output - zasób produkowany przez budynek wraz z ilością.</summary>
    public ItemStack Output => output;

    /// <summary>Definicja zasobu produkowanego przez budynek.</summary>
    public ResourceDef OutputResource => output != null ? output.Resource : null;

    /// <summary>Ilość zasobu produkowanego przez budynek.</summary>
    public int OutputAmount => output != null ? output.Amount : 0;
}