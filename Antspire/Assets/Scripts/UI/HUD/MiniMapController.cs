using UnityEngine;

public class MinimapController : MonoBehaviour
{
    
    [Header("Minimap Tracking")]
    public Transform targetToFollow; // Gracz lub kamera
    public Camera minimapCamera;
    public float followHeight = 50f;

    [Header("Minimap UI")]
    public RectTransform minimapUIPanel; // UI minimapy (stoi w miejscu)

    void Update()
    {
        if (targetToFollow != null && minimapCamera != null)
        {
            // Kamera minimapy podąża za celem
            Vector3 newPosition = targetToFollow.position;
            newPosition.y = targetToFollow.position.y + followHeight;
            minimapCamera.transform.position = newPosition;

            // Kamera patrzy prosto w dół
            minimapCamera.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
        }
    }
}