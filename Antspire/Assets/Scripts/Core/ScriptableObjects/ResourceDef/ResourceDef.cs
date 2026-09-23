using UnityEngine;

/// <summary>
/// Definicja zasobu - identyfikuje pojedynczy surowiec w grze.
/// Na razie przechowuje wyłącznie nazwę zasobu.
/// </summary>
[CreateAssetMenu(fileName = "ResourceDef", menuName = "ScriptableObjects/Production/ResourceDef")]
public class ResourceDef : ScriptableObject
{
    [Header("Podstawowe informacje")]
    [SerializeField] private string resourceName;

    /// <summary>Nazwa wyświetlana zasobu.</summary>
    public string ResourceName => resourceName;
}