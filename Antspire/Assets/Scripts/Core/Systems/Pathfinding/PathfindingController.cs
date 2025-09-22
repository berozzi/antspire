using System.Collections.Generic;
using UnityEngine;
// this script will manage pathfinding in the game
public class PathfindingController : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    public float moveCost = 1f; // cost to move to a neighboring hex
    public FindPath findPath;
    GenerateGrid generateGrid;
    HexClick hexClick;
    public GetCells getCells;
    // Gameobject
    //[SerializeField] Vector3 objectStartPosition;
    Unit unit;
    //int startX = 10;
    //int startZ = 10;

    void Awake()
    {
        findPath = new FindPath();
        //generateGrid = FindAnyObjectByType<GenerateGrid>();
        getCells = new GetCells();
        hexClick = FindAnyObjectByType<HexClick>();
    }
    void Start()
    {
        // Po wygenerowaniu siatki
       
        if (generateGrid == null)
            Debug.Log("jest nullem ten twoj grid");
        //objectStartPosition = generateGrid.CalculateWorldPosition(startX, startZ, 9.25f);
        //Debug.Log($"Pozycja startowa: {objectStartPosition}");
        FindAndShowPath();
    }

    void FindAndShowPath()
    {
        //HexCell startCell = unit.ReturnStartCell(); // position of the ant or mob starting point
        HexCell startCell = getCells.GetCell(0, 0);
        Debug.Log(startCell);// temporary
        HexCell endCell = hexClick.ReturnTargetHexcell(); // destination point. Set by clicking on a hex. Function SelectHex should return the selected HexCell.

        if (startCell != null && endCell != null)
        {
            List<HexCell> path = findPath.FindPathToTarget(startCell, endCell);

            if (path != null)
            {
                findPath.VisualizePath(path);
                Debug.Log($"Znaleziono œcie¿kê o d³ugoœci: {path.Count}");
            }
            else
            {
                Debug.Log("Brak œcie¿ki!");
            }
        }
    }
}
