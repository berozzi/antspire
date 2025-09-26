using UnityEngine;

[CreateAssetMenu(fileName = "New Biome Type", menuName = "Biomes/Biome Type Data")]
public class BiomeTypeData : ScriptableObject
{
    public string biomeName;
    public BiomeType type; // Enum for easy identification
    public string BiomeDifficulty;
    public GameObject model;
}

public enum BiomeType
{
    Forest,
    Desert,
    Tundra,
    Swamp,
    Plains,
    Mountain,
    River
}   