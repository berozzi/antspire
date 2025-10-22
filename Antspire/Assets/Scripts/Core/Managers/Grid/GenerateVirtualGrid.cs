using UnityEngine;
using UnityEngine.UI;

public class GenerateVirtualGrid : MonoBehaviour
{
    [SerializeField ]int width = 51;
    [SerializeField ]int height = 51; // because we start from 1 in loops
    float cellSize = 1f; // rozmiar pola w œwiecie

    public Cell[,] grid;

    void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"Grid dimensions must be positive! Current: {width}x{height}");
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
        }
        grid = new Cell[width, height];

        for (int x = 1; x < width; x++)
        {
            for (int y = 1; y < height; y++)
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
        var pos =  new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
        return pos;
    }
    public Cell GetCell(Vector2Int coord)
    {
        // SprawdŸ granice siatki
        if (coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height) // w GenerateVirtualGrid szerokoœæ i wysokoœæ s¹ ustawione na 50
        {
            return grid[coord.x, coord.y];
        }
        return null;
    }
}
