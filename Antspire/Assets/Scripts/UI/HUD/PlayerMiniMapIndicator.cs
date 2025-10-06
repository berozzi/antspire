using UnityEngine;

public class PlayerMinimapIndicator : MonoBehaviour
{
    [Header("Minimap Indicator Settings")]
    public RectTransform playerIndicator;
    public Transform playerTransform;

    void Update()
    {
        if (playerIndicator != null && playerTransform != null)
        {
            // Obracaj wskaünik zgodnie z rotacjπ gracza
            playerIndicator.rotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
        }
    }
}