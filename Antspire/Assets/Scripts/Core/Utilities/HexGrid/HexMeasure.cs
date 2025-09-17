using UnityEngine;

public class HexMeasure : MonoBehaviour
{
    [SerializeField] GameObject hexPrefab;

    void Start()
    {
        if (hexPrefab != null)
        {
            // Instantiate hexagon
            GameObject hex = Instantiate(hexPrefab, Vector3.zero, Quaternion.identity);
            float meshFilter = hex.GetComponent<Mesh>().bounds.size.x;
            Debug.Log(meshFilter);
            // Get renderer bounds
            //Renderer renderer = hex.GetComponent<Renderer>();
            //if (renderer != null)
            //{
            //    Bounds bounds = renderer.bounds;
            //    Debug.Log($"Hexagon size: {bounds.size}");
            //    Debug.Log($"Hexagon width (X): {bounds.size.x}");
            //    Debug.Log($"Hexagon height (Z): {bounds.size.z}");
            //}

            //// Clean up
            //Destroy(hex);
        }
        else
        {
            Debug.Log("Assign hex prefab in inspector!");
        }
    }
}