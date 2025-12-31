using UnityEngine;

[System.Serializable]
public class ResourceData
{
    public string id; // Unique identifier
    public string resourceName;
    public float x; // Position in the world
    public float y;
    public ResourceType type; // Enum for easy identification
    public int value;  // How much this resource is worth
    public bool isAvailable; // Is it currently available in the world
    //public GameObject resourcePrefab;
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
