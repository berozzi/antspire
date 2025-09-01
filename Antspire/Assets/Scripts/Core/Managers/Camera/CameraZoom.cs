using UnityEngine;

public class CameraZoom
{
    [SerializeField] private Camera cam;
    private CameraSettings cameraSettings;
    private Transform cameraTransform;
    private Vector3 originalPosition;
    private bool isZooming = false;


    public CameraZoom(CameraSettings settings, Camera camera)
    {
        cameraSettings = settings;
        cam = camera;
        cameraTransform = cam.transform;
    }
    public void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            if (!isZooming)
            {
                originalPosition = cameraTransform.position; // Zapisz oryginaln¹ pozycjê kamery
                isZooming = true;
            }

            bool isZoomingIn = scroll > 0;

            if (isZoomingIn)
            {
                ZoomTowardsCursor();
            }
            else
            {
                ZoomBackToOriginal();
            }
        }
        else
        {
            isZooming = false; // Resetuj flagê zoomowania, gdy nie ma scrolla
        }
    }

    void ZoomTowardsCursor()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int terrainLayer = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 groundPoint = hit.point;

            // P³ynne przesuniêcie kamery w kierunku punktu na ziemi
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                new Vector3(groundPoint.x, cameraSettings.minZoom, groundPoint.z),
                Time.deltaTime * cameraSettings.zoomSmoothSpeed
            );
        }
    }

    void ZoomBackToOriginal()
    {
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            originalPosition,
            Time.deltaTime * cameraSettings.zoomSmoothSpeed
        );
    }
}
