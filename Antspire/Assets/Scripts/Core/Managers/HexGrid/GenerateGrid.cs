using UnityEngine;

public class GenerateGrid : MonoBehaviour
{

    [Header("Hexagon Settings")]
    float hexSize = 9.25f;
    int gridWidth = 20;     // szerokoœæ mapy w hexach
    int gridHeight = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Generate(GameObject hexPrefab1, GameObject hexPrefab2)
    {
        // Iteracja przez wszystkie komórki w siatce
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Konwersja wspó³rzêdnych siatki na wspó³rzêdne œwiatowe
                Vector3 position = CalculateWorldPosition(x, z);
                GameObject hex;
                // Instantiate hexagonu
                if (z % 2 == 1)
                {
                     hex = Instantiate(hexPrefab2, position, Quaternion.identity, transform);
                    hex.transform.rotation = Quaternion.Euler(0, 30, 0);
                }
                else
                {
                    hex = Instantiate(hexPrefab1, position, Quaternion.identity, transform);
                    hex.transform.rotation = Quaternion.Euler(0, 30, 0);
                }

                hex.name = $"Hex_{x}_{z}"; // Nazwa hexagonu na podstawie jego wspó³rzêdnych
                HexCell cell = hex.AddComponent<HexCell>();
                cell.xPosition = x;
                cell.zPosition = z;
                //Debug.Log($"Hex position: {position} at grid ({x}, {z})");
            }
        }
    }
    public Vector3 CalculateWorldPosition(int x, int z)
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
