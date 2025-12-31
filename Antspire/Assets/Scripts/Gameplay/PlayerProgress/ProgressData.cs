using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class ProgressData
{
    // Rozdzia³y fabularne
    public int currentChapter = 1;
    public float chapterProgress = 0f; // 0-100%
    public bool[] completedQuests = new bool[20];

    // Statystyki kolonii
    public int totalAnts = 0;
    public int maxAnts = 50;
    public int deadAnts = 0;
    public int antsBorn = 0;

    // Ekonomia
    public Dictionary<ResourceType, int> resources = new();
    public int totalPheromonesProduced = 0;
    public int totalPheromonesSpent = 0;

    // Budynki
    public Dictionary<BuildingType, int> buildingLevels = new();
    public Dictionary<BuildingType, int> buildingCount = new();

    // Dyplomacja
    public float diplomacyScore = 50f; // 0-100
    

    // Postêpy technologiczne
    //public List<TechType> unlockedTechs = new();
    public int researchPoints = 0;

    // Czas gry
    public int gameDays = 1;
}
