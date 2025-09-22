using System.Collections.Generic;
using UnityEngine;

public class FindPath
{
    GetCells getCells;
    HexCell[,] grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<HexCell> FindPathToTarget(HexCell startCell, HexCell targetCell)
    {
        if (startCell == null || targetCell == null || !targetCell.isWalkable)
            return null;

        List<HexCell> openSet = new List<HexCell>();
        HashSet<HexCell> closedSet = new HashSet<HexCell>();

        openSet.Add(startCell);

        // Resetuj wartoœci dla wszystkich komórek (opcjonalnie, ale wa¿ne)
        foreach (var cell in grid)
        {
            cell.gCost = int.MaxValue;
            cell.hCost = 0;
            cell.parent = null;
        }

        startCell.gCost = 0;
        startCell.hCost = CalculateDistance(startCell, targetCell);

        while (openSet.Count > 0)
        {
            HexCell currentCell = openSet[0];

            // ZnajdŸ komórkê z najmniejszym F cost
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentCell.fCost ||
                    (openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost))
                {
                    currentCell = openSet[i];
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            // Jeœli znaleŸliœmy cel
            if (currentCell == targetCell)
            {
                return RetracePath(startCell, targetCell);
            }

            // SprawdŸ wszystkich s¹siadów
            foreach (HexCell neighbor in getCells.GetNeighbors(currentCell))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int newMovementCost = currentCell.gCost + CalculateDistance(currentCell, neighbor) * neighbor.movementCost;

                if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = CalculateDistance(neighbor, targetCell);
                    neighbor.parent = currentCell;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // Brak œcie¿ki
    }

    // Oblicz odleg³oœæ miêdzy komórkami (dla heksagonów)
    private int CalculateDistance(HexCell a, HexCell b)
    {
        int dx = Mathf.Abs(a.xPosition - b.xPosition);
        int dz = Mathf.Abs(a.zPosition - b.zPosition);

        // Proste przybli¿enie odleg³oœci dla heksagonów
        return Mathf.Max(dx, dz, Mathf.Abs(dx - dz));
    }

    // Odtwórz œcie¿kê
    private List<HexCell> RetracePath(HexCell startCell, HexCell endCell)
    {
        List<HexCell> path = new List<HexCell>();
        HexCell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();
        return path;
    }

    // Metoda do wizualizacji œcie¿ki (opcjonalnie)
    public void VisualizePath(List<HexCell> path)
    {
        foreach (HexCell cell in path)
        {
            // Zmieñ kolor/materia³ komórki, aby pokazaæ œcie¿kê
            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }
    }
}
