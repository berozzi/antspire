using UnityEngine;

public class Unit : MonoBehaviour
{
    HexCell startCell;    // hex, z którego wystartowa³
    HexCell currentCell;  // hex, na którym stoi teraz\
    Vector3 position;

    // Wywo³ujesz przy spawn'ie jednostki
    public void Initialize(HexCell spawnCell)
    {
        startCell = spawnCell;
        currentCell = spawnCell;
        transform.position = spawnCell.transform.position;
    }

    // Wywo³ujesz, kiedy jednostka zmienia pozycjê
    public void SetCurrentCell(HexCell newCell)
    {
        currentCell = newCell;
        transform.position = newCell.transform.position;
    }

    public HexCell ReturnStartCell()
    {
        startCell.xPosition = Mathf.RoundToInt(position.x / (9.25f * 1.43f));
        startCell.zPosition = Mathf.RoundToInt(position.z / (14f / 1.28f));
        return startCell; // Tymczasowo zwraca startCell
    }
}