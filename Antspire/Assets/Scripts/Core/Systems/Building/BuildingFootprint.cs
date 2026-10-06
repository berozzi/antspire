using UnityEngine;

/// <summary>
/// Zapisuje, jakie komórki siatki zajmuje postawiony budynek.
/// Komponent jest dodawany w runtime przez BuildingManager przy stawianiu,
/// dzięki czemu przy usuwaniu zwalniamy dokładnie te komórki, które były
/// zajęte (a nie komórki odpowiadające aktualnie wybranemu prefabowi).
/// </summary>
public class BuildingFootprint : MonoBehaviour
{
    /// <summary>Komórka lewego dolnego rogu footprintu (origin budynku na siatce).</summary>
    public Vector2Int Origin { get; private set; }

    /// <summary>Szerokość footprintu w komórkach (oś X).</summary>
    public int Width { get; private set; } = 1;

    /// <summary>Wysokość footprintu w komórkach (oś Z).</summary>
    public int Height { get; private set; } = 1;

    public void Initialize(Vector2Int origin, int width, int height)
    {
        Origin = origin;
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
    }
}
