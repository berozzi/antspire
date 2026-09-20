using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private string buildingId;
    [SerializeField] private string buildingName = string.Empty;
    [SerializeField] private string buildingType = string.Empty;
    [SerializeField] private BuildingCategory category; // po to aby kategoryzowa� budynki

    public string Id => buildingId;
    public BuildingCategory Category => category;

}

public enum BuildingCategory
{     
    Residential,
    Industrial,
    Agricultural,
    Barracks,
    Medical,
    Educational,
    MixedUse,
    Other
}
