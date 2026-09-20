using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    // siatke rozszerzymy p�niej do rozmiaru 80x80 czyli ��cznie 6400 kom�rek  
    [SerializeField] int width = 80;
    [SerializeField] int height = 120; 
    int widthFixed = 90, heightFixed = 135;
    float cellSize = 1f; // rozmiar pola w �wiecie

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
                // TestGridByCubes(x, y); // do test�w wizualnych
                grid[x, y] = new Cell
                {
                    position = new Vector2(worldX, worldZ),
                    isOccupied = false
                };
            }
        }
    }
    // konwersja ze �wiata 3D -> grid
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.z / cellSize); // z to g��bia
        return new Vector2Int(x, y);
    }

    // odwrotnie: grid -> pozycja w �wiecie
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        // Dodaj po�ow� cellSize �eby budynek by� W CENTRUM kom�rki a nie na po�owie
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
        if (coord.x >= 0 && coord.x < width - 2 && coord.y >= 0 && coord.y < height - 2) // -2 aby zablokowa� budowanie po prawej i g�rnej stronie
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

        // Nadaj materia�
        Renderer renderer = cube.GetComponent<Renderer>();

        // ALBO zmie� kolor materia�u
        renderer.material.color = Color.green;
    }
}
// pozosta�o dopracowa� zablokowa� stawianie budynk�w po prawej stronie i g�rnej stronie siatki zmniejszy� nieco grida
// po to aby budynki nie wystawa�y poza map� oraz �eby gracz poczu� �e jest ograniczony map� a nie �e jak si� ko�czy siatka to nadal mo�e budowa�
// dzi�ki temu miejscu mo�emy potem w miar� p�ynnie przej�� do zako�czenia mapy widzianej przez gracza