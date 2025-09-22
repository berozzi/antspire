using System.Collections.Generic;
using UnityEngine;

public class GetCells 
{
    //GenerateGrid generateGrid;
    int gridWidth = 20;
    int gridHeight = 20;
    public HexCell[,] grid;
    
    
    public HexCell GetCell(int x, int z)
    {
        grid = new HexCell[gridWidth, gridHeight];
        if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
            return grid[x, z];
        return null;
    }

    // Pobierz s¹siadów komórki (dla siatki heksagonalnej)
    public List<HexCell> GetNeighbors(HexCell cell)
    {
        List<HexCell> neighbors = new List<HexCell>();
        int x = cell.xPosition;
        int z = cell.zPosition;

        // S¹siedztwo zale¿y od tego, czy rz¹d jest parzysty czy nieparzysty
        bool oddRow = (z % 2 == 1);

        // Wspó³rzêdne s¹siadów dla siatki heksagonalnej
        Vector2Int[] neighborOffsets = oddRow ?
            new Vector2Int[] {
                new Vector2Int(0, -1), new Vector2Int(1, -1),  // góra, góra-prawo
                new Vector2Int(-1, 0), new Vector2Int(1, 0),   // lewo, prawo
                new Vector2Int(0, 1), new Vector2Int(1, 1)     // dó³, dó³-prawo
            } :
            new Vector2Int[] {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), // góra-lewo, góra
                new Vector2Int(-1, 0), new Vector2Int(1, 0),   // lewo, prawo
                new Vector2Int(-1, 1), new Vector2Int(0, 1)    // dó³-lewo, dó³
            };

        foreach (var offset in neighborOffsets)
        {
            HexCell neighbor = GetCell(x + offset.x, z + offset.y);
            if (neighbor != null && neighbor.isWalkable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
}
