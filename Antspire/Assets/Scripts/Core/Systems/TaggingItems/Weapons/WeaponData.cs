using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Inventory/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public int damage;
    public float range;
    public float attackSpeed;
    public Sprite icon;
    public Color color;
    public GameObject weaponPrefab;
}

public enum WeaponType
{
    Melee,
    Ranged,
    Magic
}