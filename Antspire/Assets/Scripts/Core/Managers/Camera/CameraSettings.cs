using UnityEngine;

[System.Serializable]
public class CameraSettings
{
    [Header("Camera Movement Settings")]
    public float edgeScrollThickness = 10f; // Thickness of the edge for scrolling
    public float moveSpeed = 20f; // Speed of camera movement
    [Header("Camera Zoom Settings")]
    public float zoomSpeed = 50f; // Speed of zooming in and out
    public float zoomSmoothSpeed = 10f; // Smoothness of zooming
    public float minZoom = 10f; // Minimum zoom level
    public float maxZoom = 100f; // Maximum zoom level
    public float initialZoom = 60f; // Initial zoom level when the game starts
    [Header("Camera Rotation Settings")]
    public float pitch = 45f;
    public float yaw = 0f;
    public bool isRotating = false;
    public Vector2 rotationLimits = new Vector2(30f, 80f); // Pitch min/max limits
    public Vector3 previousPosition;
    public float rotationSpeed = 5f; // Speed of camera rotation
}
