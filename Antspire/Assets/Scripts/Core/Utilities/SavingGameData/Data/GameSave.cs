using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class GameSave
{
    private static GameSave _instance;
    public static GameSave Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameSave();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    void Initialize()
    {
        player = new PlayerData();
        ants = new List<AntData>();
        enemies = new List<EnemyData>();
        houseData = new List<HouseData>();
        workplaceData = new List<WorkplaceData>();
        roads = new List<RoadData>();
        resources = new List<ResourceData>();
    }

    public PlayerData player;
    public List<AntData> ants;
    public List<EnemyData> enemies;
    public List<HouseData> houseData;
    public List<WorkplaceData> workplaceData;
    public List<RoadData> roads;
    public List<ResourceData> resources;

    public GameSave() { }
}
