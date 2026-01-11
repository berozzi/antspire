using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector2 position;
    public bool isOccupied;

    // NOWE: Referencja do budynku który zajmuje tê komórkê
    [System.NonSerialized] // Nie serializuj GameObject reference
    public GameObject building;
}