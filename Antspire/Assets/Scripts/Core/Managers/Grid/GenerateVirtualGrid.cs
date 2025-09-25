using UnityEngine;

public class GenerateVirtualGrid : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    float cellSize = 1f; // rozmiar pola w œwiecie

    Cell[,] grid;

    void Start()
    {
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell
                {
                    position = new Vector2Int(x, y),
                    isOccupied = false
                };
            }
        }
    }

    // konwersja ze œwiata 3D -> grid
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.z / cellSize); // z to g³êbia
        return new Vector2Int(x, y);
    }

    // odwrotnie: grid -> pozycja w œwiecie
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }
}
