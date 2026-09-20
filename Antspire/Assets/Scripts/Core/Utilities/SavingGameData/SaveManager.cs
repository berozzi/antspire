using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{

    string customPath = "C:/Users/GracjanCode/Desktop/Ants/Saves/gameSave.json";
    // HashSet = brak duplikat�w, szybkie dodawanie/usuwanie
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

    // Metoda zapisuj�ca wszystko
    public static GameSave SaveAll()
    {
        GameSave gameSave = GameSave.Instance;
        // GameSave gameSave = GameSave.Instance.Initialize(); je�li tworz� nowego save'a

        foreach (ISaveable saveable in saveables)
        {
            object data = saveable.GetSaveData();

            switch (data)
            {
                case AntData ant: gameSave.ants.Add(ant); break;
                //case StructureData building: gameSave.structures.Add(building); break;
                //case TunnelData tunnel: gameSave.tunnels.Add(tunnel); break;
                case WorkplaceData workplace: gameSave.workplaceData.Add(workplace); break;
            }
        }

        return gameSave;
    }
    public Vector2Int GetCurrentPosition(Vector3 pos)
    {
        GridManager gridManager = FindAnyObjectByType<GridManager>();
        if (gridManager != null)
        {
            return gridManager.WorldToGrid(pos);
        }
        else
        {
            Debug.LogError("GridManager not found!");
            return Vector2Int.zero;
        }
    }
    public void SaveGame()
    {
        var gameSave = SaveAll();

        string json = JsonUtility.ToJson(gameSave, true);
        File.WriteAllText(customPath, json);
        Debug.Log("Saved Game");
    }

    public void LoadGame()
    {
        if (!File.Exists(customPath))
        {
            Debug.LogWarning("No save file found at: " + customPath);
            return;
        }

        string json = File.ReadAllText(customPath);
        GameSave loadedGameSave = JsonUtility.FromJson<GameSave>(json);
        if (loadedGameSave == null)
        {
            Debug.LogWarning("Save file is empty or invalid: " + customPath);
            return;
        }

        GameSave.Instance.player = loadedGameSave.player;
        GameSave.Instance.resources = loadedGameSave.resources;
        Debug.Log("Game Loaded with New Input System!");
    }
}
// wystarczy to jedynie poszerza� w switchu o kolejne przypadki
// w GameSave dodawa� listy do przechowywania danych
// i tworzy� odpowiednie klasy danych do przechowywania informacji o obiektach
// to jest bardzo elastyczne podej�cie dzi�ki kt�remu unikamy niepotrzebnego kodu oraz dziedziczenia

// ka�dy nast�pny typ obiektu Saveable wymaga jedynie dodania kolejnego case'a w switchu powy�ej oraz rejestracji