using UnityEngine;

public class GridManager : MonoBehaviour
{
    // siatkę rozszerzymy później do rozmiaru 80x80 czyli łącznie 6400 komórek
    [SerializeField] int width = 80;
    [SerializeField] int height = 120;
    float cellSize = 1f; // rozmiar komórki - używany jako fallback, gdy brak collidera mapy

    /// <summary>
    /// Komórki w układzie jednowymiarowym: indeks = x * height + y.
    /// Pole jest prywatne i nieuserializowane, bo:
    /// 1) Unity nie obsługuje tablic dwuwymiarowych (stąd poprzedni warning),
    /// 2) grid i tak powstaje w runtime (InitializeGrid w Start),
    ///    więc zapisywanie tysięcy komórek do sceny tylko by je przeterminowywało.
    /// </summary>
    private Cell[] cells;

    /// <summary>Collider mapy, na której leży GridManager (np. PlaneMap) - źródło obszaru siatki.</summary>
    private Collider mapCollider;

    public int Width => width;
    public int Height => height;

    /// <summary>Lewy dolny róg siatki w świecie (światowa pozycja komórki [0, 0]).</summary>
    public Vector2 MapOrigin
    {
        get
        {
            MapArea area = GetMapArea();
            return new Vector2(area.min.x, area.min.z);
        }
    }

    private void Awake()
    {
        mapCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// Odpalany po (ponownej) budowie siatki. Umożliwia innym systemom
    /// (np. BuildingManager) zajęcie komórek pod budynkami już stojącymi
    /// w scenie - niezależnie od kolejności Start() między obiektami.
    /// </summary>
    public event System.Action GridReady;

    void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        cells = new Cell[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // position = środek komórki w świecie, ten sam układ co GridToWorld
                Vector3 cellCenter = GridToWorld(new Vector2Int(x, y));
                cells[IndexOf(x, y)] = new Cell
                {
                    position = new Vector2(cellCenter.x, cellCenter.z),
                    isOccupied = false
                };
            }
        }

        GridReady?.Invoke();
    }

    /// <summary>Spłaszczony indeks komórki (x, y) w tablicy jednowymiarowej.</summary>
    private int IndexOf(int x, int y) => x * height + y;

    /// <summary>Czy siatka została już zbudowana (komórki dostępne).</summary>
    public bool IsInitialized => cells != null;

    /// <summary>Czy współrzędne mieszczą się w granicach siatki.</summary>
    private bool IsInside(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    /// <summary>Pobiera komórkę po współrzędnych siatki (null poza granicami).</summary>
    public Cell GetCell(int x, int y)
    {
        if (cells == null || !IsInside(x, y)) return null;
        return cells[IndexOf(x, y)];
    }

    // konwersja ze świata 3D -> grid
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        MapArea area = GetMapArea();
        int x = Mathf.FloorToInt((worldPos.x - area.min.x) / area.cellSizeX);
        int y = Mathf.FloorToInt((worldPos.z - area.min.z) / area.cellSizeZ); // z to głębia
        return new Vector2Int(x, y);
    }

    // odwrotnie: grid -> pozycja w świecie
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        MapArea area = GetMapArea();

        // Zwracamy ŚRODEK komórki, żeby budynek stał w centrum pola a nie na krawędzi
        return new Vector3(
            area.min.x + (gridPos.x + 0.5f) * area.cellSizeX,
            0,
            area.min.z + (gridPos.y + 0.5f) * area.cellSizeZ
        );
    }

    public Cell GetCell(Vector2Int coord)
    {
        // -2 aby zablokować budowanie po prawej i górnej stronie
        if (coord.x >= 0 && coord.x < width - 2 && coord.y >= 0 && coord.y < height - 2)
        {
            if (cells == null) return null;
            return cells[IndexOf(coord.x, coord.y)];
        }
        return null;
    }

    /// <summary>Obszar mapy w świecie oraz faktyczny rozmiar jednej komórki.</summary>
    private struct MapArea
    {
        public Vector3 min;      // lewy dolny róg mapy (oś Y zerowana - grid leży na płaszczyźnie)
        public float cellSizeX;  // szerokość komórki w osi X
        public float cellSizeZ;  // szerokość komórki w osi Z
    }

    /// <summary>
    /// Siatka pokrywa dokładnie mapę, na której leży GridManager.
    /// Origin i rozmiar komórki liczymy z collidera mapy (np. PlaneMap),
    /// dzięki czemu współrzędne świata i siatki zawsze się zgadzają -
    /// niezależnie od pozycji, skali czy rozmiaru siatki.
    /// </summary>
    private MapArea GetMapArea()
    {
        if (mapCollider != null)
        {
            Bounds bounds = mapCollider.bounds;
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);

            return new MapArea
            {
                min = new Vector3(bounds.min.x, 0f, bounds.min.z),
                cellSizeX = Mathf.Max(0.0001f, bounds.size.x) / safeWidth,
                cellSizeZ = Mathf.Max(0.0001f, bounds.size.z) / safeHeight
            };
        }

        // Fallback (brak collidera): siatka wycentrowana na transformie GridManagera.
        Vector3 center = transform.position;
        float mapWidth = Mathf.Max(1, width) * cellSize;
        float mapHeight = Mathf.Max(1, height) * cellSize;

        return new MapArea
        {
            min = new Vector3(center.x - mapWidth * 0.5f, 0f, center.z - mapHeight * 0.5f),
            cellSizeX = cellSize,
            cellSizeZ = cellSize
        };
    }

    void TestGridByCubes(int x, int y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x, 0, y);
        cube.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);

        // Kolor przez PropertyBlock zamiast renderer.material - getter "material"
        // tworzy nową instancję materiału na każdą kostkę (wyciek + batching).
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_Color"), Color.green);
            renderer.SetPropertyBlock(block);
        }
    }
}
// pozostało dopracować zablokować stawianie budynków po prawej stronie i górnej stronie siatki zmniejszyć nieco grida
// po to aby budynki nie wystawały poza mapę oraz żeby gracz poczuł że jest ograniczony mapą a nie że jak się kończy siatka to nadal może budować
// dzięki temu miejscu możemy potem w miarę płynnie przejść do zakończenia mapy widzianej przez gracza
