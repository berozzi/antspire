using UnityEngine;

public class LocateHexSettings 
{
    [Header("Poruszanie siê po hex grid")]
    public Vector3 startPosition; // pocz¹tkowa pozycja gameObjectu w hex grid
    public Vector3 endPosition; // koñcowa pozycja gameObjectu w hex grid
    public float moveSpeed = 5f; // prêdkoœæ poruszania siê miêdzy hexami
}
