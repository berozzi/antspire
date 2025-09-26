using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building Type", menuName = "Buildings/Building Type Data")]
public class BuildingTypesData : ScriptableObject
{
    public string buildingName; 
    public BuildingType type; // Enum for easy identification
    public int size;
    public float cost;
    public Sprite icon;
    public GameObject model;

}

public enum BuildingType
{
    Tunnel,
    Residential,
    Mine,
    Factory,
    Industrial,
}