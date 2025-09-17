using UnityEngine;

public class MoveGameObject : MonoBehaviour
{
    LocateHex locateHex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        locateHex.MoveToCords(2, 3);
    }
}
