using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] NavMeshSurface surface;

    /// <summary>Ostatnie asynchroniczne budowanie NavMeshu.</summary>
    private AsyncOperation pendingBuild;

    public void RebuildNavMesh()
    {
        // Opcjonalnie: opóźnienie rebuildu, aby nie rebuildować kilka razy na klatkę
        CancelInvoke(nameof(BuildDelayed));
        Invoke(nameof(BuildDelayed), 0.1f);
    }

    private void BuildDelayed()
    {
        if (surface == null)
        {
            Debug.LogError("NavMeshManager: brak przypisanego NavMeshSurface.", this);
            return;
        }

        // Pierwsze budowanie (brak danych) - jednorazowe i synchroniczne.
        if (surface.navMeshData == null)
        {
            surface.BuildNavMesh();
            return;
        }

        // Poprzedni asynchroniczny rebuild jeszcze trwa - odłóż próbę na później,
        // bo NavMeshBuilder nie aktualizuje tych samych danych równolegle.
        if (pendingBuild != null && !pendingBuild.isDone)
        {
            CancelInvoke(nameof(BuildDelayed));
            Invoke(nameof(BuildDelayed), 0.1f);
            return;
        }

        // UpdateNavMesh buduje dane asynchronicznie (zadanie w tle) i nie blokuje klatki,
        // w odróżnieniu od BuildNavMesh().
        pendingBuild = surface.UpdateNavMesh(surface.navMeshData);
    }
}
