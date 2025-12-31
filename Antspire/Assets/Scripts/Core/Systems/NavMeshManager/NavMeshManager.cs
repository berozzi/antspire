using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] NavMeshSurface surface;

    public void RebuildNavMesh()
    {
        // Opcjonalnie: opóŸnienie rebuildu, aby nie rebuildowaæ kilka razy na klatkê
        CancelInvoke(nameof(BuildDelayed));
        Invoke(nameof(BuildDelayed), 0.1f);
    }

    private void BuildDelayed()
    {
        surface.BuildNavMesh();
    }
}
