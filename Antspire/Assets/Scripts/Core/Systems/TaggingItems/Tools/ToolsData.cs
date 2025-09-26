using UnityEngine;

[CreateAssetMenu(fileName = "NewToolsData", menuName = "Inventory/Tools Data")]
public class ToolsData : ScriptableObject
{
    public string toolName;
    public ToolType toolType;
    public int durability;
    public float actionSpeed;
    public Sprite icon;
    public Color color;
    public GameObject toolPrefab;
}

public enum ToolType
{
    Axe,
    Pickaxe,
    Shovel,
    Hoe,
    WateringCan,
    FishingRod,
    Hammer
}