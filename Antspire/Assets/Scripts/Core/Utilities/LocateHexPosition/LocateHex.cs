using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LocateHex : MonoBehaviour
{
    LocateHexSettings hexSettings;
    

    public void MoveToCords(int x, int z)
    {
        hexSettings.endPosition = new Vector3(x, 0, z);
        float step = hexSettings.moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, hexSettings.endPosition, step);
    }

    //void SelectHex(int x, int z)
    //{
        
    //}

}