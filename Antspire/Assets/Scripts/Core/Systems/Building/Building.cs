using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private string buildingId;
    [SerializeField] private string displayName;
    [SerializeField] private BuildingCategory category;

    public string Id => buildingId;
    public string DisplayName => displayName;
    public BuildingCategory Category => category;
}

public enum BuildingCategory
{     
    Residential,
    Commercial,
    Industrial,
    Agricultural,
    Recreational,
    Institutional,
    MixedUse,
    Other
}
