using UnityEngine;

public class HexCell : MonoBehaviour
{
    public int xPosition; // x coordinate in the grid
    public int zPosition; // z coordinate in the grid
    public bool isWalkable = true;
    public int movementCost = 1;

    // Dane dla A*
    public int gCost;
    public int hCost;
    public HexCell parent;

    public int fCost { get { return gCost + hCost; } }

    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        // Mo¿esz dodaæ zmianê koloru/materia³u dla wizualizacji
    }
}
