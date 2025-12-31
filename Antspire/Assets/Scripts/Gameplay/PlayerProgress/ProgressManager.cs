using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] PheromoneManager pheromoneManager;
    int pheromones = 0;
    //int buildingLevel = 1;
    // Upgrades targets
    public Workplace workplaceToUpgrade;
    public House houseToUpgrade;
    public Ant antToUpgrade;
    int upgradeCost = 500;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pheromones = (int)pheromoneManager.CurrentPheromones;
        
    }

    void UpgradeWorkplace(Workplace workplace)
    {
        if (CanUpgradeWorkplace(workplace))
        {
            workplace.Level++;
            workplace.UpgradeIncome(10f * workplace.Level);
            pheromoneManager.SpendPheromones(GetUpgradeCost(workplace));
        }
    }

    int GetUpgradeCost(Workplace w)
    {
        return upgradeCost * w.Level;
    }

    public bool CanUpgradeWorkplace(Workplace w)
    {
        return pheromones > GetUpgradeCost(w) && workplaceToUpgrade.Level < 5;
    }

    //void UpgradeHouse(House house)
    //{
    //    if (pheromones > GetUpgradeCost(house))
    //    {
    //        house.Level++;
    //        pheromoneManager.SpendPheromones(GetUpgradeCost(house));
    //    }
    //}
}
