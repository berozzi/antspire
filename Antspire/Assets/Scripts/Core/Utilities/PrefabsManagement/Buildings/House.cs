using UnityEngine;

public class House : MonoBehaviour, ISaveable, IClickable
{
    [Header("House Properties")]
    [SerializeField] private string houseName = "Ant House";
    [SerializeField] private string houseDescription = "A cozy house for ants.";
    [SerializeField] private int level = 1;

    public string HouseName => houseName;
    public string HouseDescription => houseDescription;
    public int Level
    {
           get { return level; }
        set { level = value; }
    }

    object ISaveable.GetSaveData()
    {
        return new HouseData
        {
            Name = this.HouseName,
            Description = this.HouseDescription,
            Level = this.Level
        };
    }
    void ISaveable.LoadDataFromSave(object data)
    {
        if (data is HouseData houseData)
        {
            this.houseName = houseData.Name;
            this.houseDescription = houseData.Description;
            this.Level = houseData.Level;
        }
    }

    public void OnClick()
    {
        Debug.Log($"House {HouseName} clicked.");
    }

    public ClickableData GetClickableData()
    {
        return new ClickableData
        {
            Name = this.HouseName,
            Type = "House",
            Description = this.HouseDescription,
            Level = this.Level,
            Icon = null, // Assign appropriate icon here
            Stats = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Level", Level.ToString() }
                // Add more stats as needed
            }
        };
    }
}
