using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;


public class HexGridController : MonoBehaviour
{
    [Header("Grid Settings")]
    //public NavMeshSurface navMeshSurface; 
    public GameObject hexPrefab;
    public GameObject hexPrefab2;
    int gridWidth = 20;     // szerokoœæ mapy w hexach
    int gridHeight = 20;    // wysokoœæ mapy w hexach
    float hexSize = 9.25f;     // rozmiar hexa (od œrodka do wierzcho³ka), przy tej wartoœci 9.25f, hexy siê stykaj¹
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Iteracja przez wszystkie komórki w siatce
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // 1. Konwersja wspó³rzêdnych siatki na wspó³rzêdne œwiatowe
                Vector3 position = CalculateWorldPosition(x, z);

                // 2. Instantiate hexagonu
                if (z % 2 == 1)
                {
                    GameObject hex = Instantiate(hexPrefab2, position, Quaternion.identity, transform);
                    hex.name = $"Hex_{x}_{z}";
                    hex.transform.rotation = Quaternion.Euler(0, 30, 0);
                }
                else
                {
                    GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);
                    hex.name = $"Hex_{x}_{z}";
                    hex.transform.rotation = Quaternion.Euler(0, 30, 0);
                }
            }
        }
    }

    Vector3 CalculateWorldPosition(int x, int z)
    {
        // Obliczenie pozycji w przestrzeni 3D
        float xPos = x * hexSize * 1.43f; // 1.5 = 2 * 0.75 (offset w osi X)
        float zPos = z * 14f / 1.28f; // Odstêp w osi Z

        // Przesuniêcie co drugiego rzêdu dla efektu naprzemiennego
        if (z % 2 == 1)
        {
            xPos += hexSize * 0.75f;
        }

        return new Vector3(xPos, 0f, zPos);
    }
}