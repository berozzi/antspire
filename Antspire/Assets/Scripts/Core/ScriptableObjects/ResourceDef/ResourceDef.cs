using UnityEngine;

/// <summary>
/// Definicja zasobu - identyfikuje pojedynczy surowiec w grze.
/// Każdy zasób to osobny plik .asset (np. Wood.asset, Stone.asset, Pheromones.asset),
/// dzięki czemu można go swobodnie dodawać i rozpoznawać po nazwie pliku.
/// </summary>
[CreateAssetMenu(fileName = "ResourceDef", menuName = "ScriptableObjects/Production/ResourceDef")]
public class ResourceDef : ScriptableObject
{
    [Header("Podstawowe informacje")]
    [SerializeField] private string resourceName;

    public string ResourceName => string.IsNullOrWhiteSpace(resourceName) ? name : resourceName;
    public string Id => name;
    public override string ToString() => ResourceName;
}
