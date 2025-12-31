using UnityEngine;
using UnityEngine.UI;

public class GenerateVirtualGrid : MonoBehaviour
{
    // siatke rozszerzymy póŸniej do rozmiaru 80x80 czyli ³¹cznie 6400 komórek 
    public Transform planeMap; // Przeci¹gnij Plane tutaj
    [SerializeField] int width = 101;
    [SerializeField] int height = 151; 
    float cellSize = 1f; // rozmiar pola w œwiecie

    [SerializeField] Vector2Int startWorldPosition = new Vector2Int(50, 242); // x - x/20, z - z/2

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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int worldX = startWorldPosition.x + x;
                int worldZ = startWorldPosition.y + y; // u¿ywamy y jako Z w œwiecie 3D
                //Debug.Log($"Grid Cell [{x},{y}] corresponds to World Position ({worldX}, 0, {worldZ})");
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
        // Dodaj po³owê cellSize ¿eby budynek by³ W CENTRUM komórki a nie na po³owie
        float offsetX = cellSize * 0.5f;
        float offsetZ = cellSize * 0.5f;

        return new Vector3(
            gridPos.x * cellSize + offsetX,
            0,
            gridPos.y * cellSize + offsetZ
        );
    }
    public Cell GetCell(Vector2Int coord)
    {
        if (coord.x > 0 && coord.x < width && coord.y > 0 && coord.y < height)
        {
            return grid[coord.x, coord.y];
        }
        return null;
    }
}
