using Unity.AI.Navigation;
using UnityEngine;

public class RefreshNavMesh : MonoBehaviour
{
    public NavMeshSurface surface;

    void Start()
    {
        // Pobierz komponent z tego samego obiektu
        surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    public void RebuildNavMesh()
    {
        surface.BuildNavMesh();
    }
}
// nale¿y przypisaæ funkcjê RebuildNavMesh do zdarzenia, które powinno wywo³aæ odœwie¿enie NavMesh, np. po zakoñczeniu budowy lub usuniêciu struktury.
// zajmiemy siê tym póŸniej w GameState.cs
