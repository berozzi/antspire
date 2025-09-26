using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Materials/Material Data")]
public class MaterialsData : ScriptableObject
{
    public string materialName; 
    public MaterialType type; // Enum for easy identification
    public Sprite icon;
    public GameObject model;
    // public Crafting recipe;
}

public enum MaterialType
{
    Concrete,
    Hedghehogspikes,
    Metal,
    Plastic,
    Rubber,
    Feromites,
}