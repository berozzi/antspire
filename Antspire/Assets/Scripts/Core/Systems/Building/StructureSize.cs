using System.Collections.Generic;
using UnityEngine;

public class StructureSize : MonoBehaviour
{
    [Header("Building Dimensions")]
    [Tooltip("Ile komórek zajmuje w osi X")]
    public int width = 1;

    [Tooltip("Ile komórek zajmuje w osi Z")]
    public int height = 1;

    [Header("Optional: Custom Shape")]
    [Tooltip("Zamiast prostok¹ta, mo¿esz zdefiniowaæ custom kszta³t (relatywne offsety od origin)")]
    public List<Vector2Int> customShape = new List<Vector2Int>();

    public bool UseCustomShape => customShape != null && customShape.Count > 0;

    // Pobierz wszystkie komórki które budynek zajmuje
    public List<Vector2Int> GetOccupiedCells(Vector2Int originCell)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (UseCustomShape)
        {
            foreach (Vector2Int offset in customShape)
            {
                cells.Add(originCell + offset);
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells.Add(new Vector2Int(originCell.x + x, originCell.y + y));
                }
            }
        }

        return cells;
    }
}
