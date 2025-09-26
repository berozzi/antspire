using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "Inventory/Food Data")]
public class FoodData : ScriptableObject
{
    public string foodName;
    public FoodType foodType;
    public int hungerRestorationValue;
    public float eatingTime;
    public Sprite icon;
    public Color color;
    public GameObject foodPrefab;
}
public enum FoodType
{
    Fruit,
    Vegetable,
    Meat,
    Dairy,
    Grain
}   
