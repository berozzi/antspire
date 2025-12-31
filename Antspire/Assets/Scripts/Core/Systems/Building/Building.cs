using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private string buildingId;
    [SerializeField] private string buildingName { get; set; } = string.Empty;
    [SerializeField] private string buildingType { get; set; } = string.Empty;
    [SerializeField] private BuildingCategory category; // po to aby kategoryzowaæ budynki

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
