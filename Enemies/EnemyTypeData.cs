using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Type", menuName = "Enemies/Enemy Type Data")]
public class EnemyTypeData : ScriptableObject
{
    
    public string enemyName;
    public EnemyType type; // Enum for easy identification
    public string enemyDifficulty;
    public int health;
    public float speed;
    public int damage;
    public Sprite icon;
    public GameObject model;
}

public enum EnemyType
{
    Termite,
    Wasp,
    Bee,
    Hornet,
    Worm,
    Ant,
    Spider,
    Ladybug
}   