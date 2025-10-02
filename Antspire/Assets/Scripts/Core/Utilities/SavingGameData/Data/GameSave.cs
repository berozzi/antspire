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
        structures = new List<StructureData>();
        roads = new List<RoadData>();
    }

    public PlayerData player;
    public List<AntData> ants;
    public List<EnemyData> enemies;
    public List<StructureData> structures;
    public List<RoadData> roads;

    public GameSave() { }
}
