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
                //case StructureData building: gameSave.structures.Add(building); break;
                //case TunnelData tunnel: gameSave.tunnels.Add(tunnel); break;
                case WorkplaceData workplace: gameSave.workplaceData.Add(workplace); break;
            }
        }

        return gameSave;
    }
    public Vector2Int GetCurrentPosition(Vector3 pos)
    {
        GenerateVirtualGrid gridManager = FindAnyObjectByType<GenerateVirtualGrid>();
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
}
// wystarczy to jedynie poszerzaæ w switchu o kolejne przypadki
// w GameSave dodawaæ listy do przechowywania danych
// i tworzyæ odpowiednie klasy danych do przechowywania informacji o obiektach
// to jest bardzo elastyczne podejœcie dziêki któremu unikamy niepotrzebnego kodu oraz dziedziczenia

// ka¿dy nastêpny typ obiektu Saveable wymaga jedynie dodania kolejnego case'a w switchu powy¿ej oraz rejestracji