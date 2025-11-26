using Unity.AI.Navigation;
using UnityEngine;

public class Tunnel : MonoBehaviour
{
    [Header("Tunnel Settings")]
    public bool isWalkable = true;
    //RefreshNavMesh refreshNavMesh;

    private void Start()
    {
        //Debug.Log($"Tunnel '{gameObject.name}' initialized. Walkable: {isWalkable}");
        // Automatycznie oznacz jako chodzalne w NavMesh
        if (isWalkable && IsProperlyPlacedInWorld())
        {
            OnPlaced();
            SetupNavMeshWalkable();
        }
    }

    private void SetupNavMeshWalkable()
    {
        // Upewnij siê ¿e collider jest triggerem (dla NavMesh)
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true; // Wa¿ne dla NavMesh!
        }

        // Dodaj NavMeshModifier jeœli nie ma
        NavMeshModifier modifier = GetComponent<NavMeshModifier>();
        if (modifier == null)
        {
            modifier = gameObject.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = 0; // 0 = Walkable w NavMesh
        }
    }

    private void OnDestroy()
    {
        if (isWalkable && IsProperlyPlacedInWorld())
        {
            OnRemoved();
        }
    }

    private bool IsProperlyPlacedInWorld()
    {
        return gameObject.scene.IsValid() && transform.parent != null;
    }

    private void OnPlaced()
    {
        //RefreshNavMesh.RebuildNavMesh();
        //NavMeshManager.RequestNavMeshUpdate();
    }

    private void OnRemoved()
    {
        //RefreshNavMesh.RebuildNavMesh();
        //sNavMeshManager.RequestNavMeshUpdate();
    }

    // te dwie metody nale¿y dodaæ do PlaceDownStrucutre i do RemoveStructure w odpowiednich miejscach
}
