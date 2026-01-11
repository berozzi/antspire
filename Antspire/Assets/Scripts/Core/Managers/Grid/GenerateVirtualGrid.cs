using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GenerateVirtualGrid : MonoBehaviour
{
    // siatke rozszerzymy póŸniej do rozmiaru 80x80 czyli ³¹cznie 6400 komórek  
    [SerializeField] int width = 80;
    [SerializeField] int height = 120; 
    int widthFixed = 90, heightFixed = 135;
    float cellSize = 1f; // rozmiar pola w œwiecie

    public Cell[,] grid;
    void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        //Debug.Log($"Grid dimensions: {width}x{height}");
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int worldX = widthFixed + x;  // 11 + x
                int worldZ = heightFixed + y;  // 16 + y
                // TestGridByCubes(x, y); // do testów wizualnych
                grid[x, y] = new Cell
                {
                    position = new Vector2(worldX, worldZ),
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
        if (coord.x >= 0 && coord.x < width - 2 && coord.y >= 0 && coord.y < height - 2) // -2 aby zablokowaæ budowanie po prawej i górnej stronie
        {
            return grid[coord.x, coord.y];
        }
        return null;
    }

    void TestGridByCubes(int x, int y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x, 0, y);
        cube.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);

        // Nadaj materia³
        Renderer renderer = cube.GetComponent<Renderer>();

        // ALBO zmieñ kolor materia³u
        renderer.material.color = Color.green;
    }
}
// pozosta³o dopracowaæ zablokowaæ stawianie budynków po prawej stronie i górnej stronie siatki zmniejszyæ nieco grida
// po to aby budynki nie wystawa³y poza mapê oraz ¿eby gracz poczu³ ¿e jest ograniczony map¹ a nie ¿e jak siê koñczy siatka to nadal mo¿e budowaæ
// dziêki temu miejscu mo¿emy potem w miarê p³ynnie przejœæ do zakoñczenia mapy widzianej przez gracza