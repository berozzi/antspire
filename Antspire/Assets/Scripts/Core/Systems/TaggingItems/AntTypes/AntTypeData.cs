using UnityEngine;

[CreateAssetMenu(fileName = "New Ant Type", menuName = "Ants/Ant Type Data")]
public class AntTypeData : ScriptableObject
{
    public string antTypeName;
    public AntType type; // Enum for easy identification
    public Sprite icon;
    public GameObject model;
}

public enum AntType
{
    Worker,
    Soldier,
    Scout,
    Archer,
    Medic,
    Larva,
    Pupa,
    Apprentice,
}