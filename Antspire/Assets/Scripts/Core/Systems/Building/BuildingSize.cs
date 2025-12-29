using UnityEngine;
public class BuildingSize : MonoBehaviour
{
    public int width;
    public int height;
    //GenerateVirtualGrid grid;

    //private void Awake()
    //{
    //    grid = FindAnyObjectByType<GenerateVirtualGrid>();
    //}
    public bool CanPlaceBuilding(int sizeX, int sizeY)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                // SprawdŸ czy komórka jest poza granicami siatki
                if (x >= width || y >= height)
                {
                    return false;
                }
                
            }
        }
        return true;
    }
}
