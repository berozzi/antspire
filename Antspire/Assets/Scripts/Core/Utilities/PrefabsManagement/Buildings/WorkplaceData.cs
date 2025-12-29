using System;
using UnityEngine;
[Serializable]
public class WorkplaceData 
{
    public string workplaceName;
    public float baseIncomePerSecond;
    public bool generatesIncome;
    public bool isActive;
    public WorkplaceType workplaceType;
    public int capacity = 1;
    public int level = 1;
}

public enum WorkplaceType
{
    Farm,
    Factory,
    Medical,
    ResearchLab,
    Military,
    Storage,
    Other
}