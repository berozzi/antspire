using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Resources/Resource Data")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public ResourceType type; // Enum for easy identification
    public int value;         // How much this resource is worth
    public Sprite icon;
    public GameObject resourcePrefab;
    public Color resourceColor;
}
public enum ResourceType
{
    Wood,
    Stone,
    Dirt,
    Sand,
    Leaf,
    Twig,
    Clay,
    Grass,
    FlowerDye,
    Resin,
    Chitin
}