using UnityEngine;
using static UnityEditor.SceneView;

public class CameraManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Camera cam;
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField] CameraSettings cameraSettings;

    float localZoom; // Current zoom level
    void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        if (cameraZoom == null)
        {
            cameraZoom = new CameraZoom(cameraSettings, cam);
            localZoom = cameraSettings.initialZoom;
        }
        
    }

    void Start()
    {
        localZoom = cam.transform.position.y;
    }

    void Update()
    {
       
        cameraZoom.Zoom();
    }
}
