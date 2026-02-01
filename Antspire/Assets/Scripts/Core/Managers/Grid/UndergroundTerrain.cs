using UnityEngine;

public class UndergroundTerrain : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float tileSize = 1f;

    [Header("Terrain Generation")]
    [Range(0f, 1f)] public float stoneThreshold = 0.8f;
    [Range(0f, 1f)] public float sandThreshold = 0.3f;
    public float noiseScale = 0.05f;

    [Header("Visualization")]
    public Material dirtMaterial;
    public Material sandMaterial;
    public Material stoneMaterial;
    
    public void GenerateTerrainByPerlin(int x, int y, Vector3 planePosition)
    {
        // Oblicz pozycję względem Plane
        Vector3 tilePosition = planePosition + new Vector3(
            x * tileSize - (gridWidth * tileSize) / 2f + tileSize / 2f,
            0.5f, // Lekko ponad Plane
            y * tileSize - (gridHeight * tileSize) / 2f + tileSize / 2f
        );

        // Generuj wartość Perlin Noise
        float noiseValue = Mathf.PerlinNoise(
            x * noiseScale + 100f,
            y * noiseScale + 100f
        );

        // Określ typ terenu
        string terrainType;
        Material tileMaterial;

        if (noiseValue > stoneThreshold)
        {
            terrainType = "Stone";
            tileMaterial = stoneMaterial;
        }
        else if (noiseValue < sandThreshold)
        {
            terrainType = "Sand";
            tileMaterial = sandMaterial;
        }
        else
        {
            terrainType = "Dirt";
            tileMaterial = dirtMaterial;
        }
        // aby zwiększyć różnorodność terenu, można dodać więcej progów i typów terenu

        // Tworzymy płytkę terenu
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.transform.position = tilePosition;
        tile.transform.localScale = new Vector3(tileSize, 1f, tileSize);
        tile.transform.parent = this.transform;
        tile.name = $"Tile_{x}_{y}_{terrainType}";

        // Przypisz materiał
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null && tileMaterial != null)
        {
            renderer.material = tileMaterial;
        }

        // Dodaj komponent Tile z informacjami
        TerrainTile tileInfo = tile.AddComponent<TerrainTile>();
        tileInfo.type = terrainType;
        tileInfo.isDiggable = (terrainType != "Stone");

        // Dopasuj kolor do typu (jeśli brak materiałów)
        if (tileMaterial == null)
        {
            if (terrainType == "Stone") renderer.material.color = Color.gray;
            else if (terrainType == "Sand") renderer.material.color = Color.yellow;
            else renderer.material.color = new Color(0.4f, 0.2f, 0f); // brąz
        }
    }
}
