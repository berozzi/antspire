using UnityEngine;
[System.Serializable]
public class BuildingData : MonoBehaviour
{
    public BuildingType buildingType;
    public Vector2Int position;
    public string buildingName;
    public int capacity;
    public bool isInUse;
    public string prefabName;
}

public enum BuildingType
{
    House = 0,
    Farm = 1,
    Barracks = 2,
    Tower = 3
}