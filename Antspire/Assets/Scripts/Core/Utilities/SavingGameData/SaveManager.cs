using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{

    string customPath = "C:/Users/GracjanCode/Desktop/Ants/Saves/gameSave.json";
    // HashSet = brak duplikatów, szybkie dodawanie/usuwanie
    private static readonly HashSet<ISaveable> saveables = new();

    public static void Register(ISaveable s)
    {
        saveables.Add(s);
        Debug.Log($"Registered saveable: {s}");
    }

    public static void Unregister(ISaveable s)
    {
        saveables.Remove(s);
    }

    // Wszystkie saveable
    public static IEnumerable<ISaveable> All => saveables;

    // Metoda zapisuj¹ca wszystko
    public static GameSave SaveAll()
    {
        GameSave gameSave = GameSave.Instance;
        // GameSave gameSave = GameSave.Instance.Initialize(); jeœli tworzê nowego save'a

        foreach (ISaveable saveable in saveables)
        {
            object data = saveable.GetSaveData();

            switch (data)
            {
                case AntData ant: gameSave.ants.Add(ant); break;
                case StructureData building: gameSave.structures.Add(building); break;
                //case TunnelData tunnel: gameSave.tunnels.Add(tunnel); break;
            }
        }

        return gameSave;
    }

    public void SaveGame()
    {
        var gameSave = SaveAll();

        string json = JsonUtility.ToJson(gameSave, true);
        File.WriteAllText(customPath, json);
        Debug.Log("Saved Game");
    }
}
