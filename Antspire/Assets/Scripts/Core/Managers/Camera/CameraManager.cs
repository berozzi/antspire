using UnityEngine;

/// Zarządza kamerą gry (ruch i zoom). Wejście pobiera z InputManagera.
public class CameraManager : MonoBehaviour
{
    [Header("Camera settings")]
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float zoomSpeed = 20f;
    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 50f;
    [SerializeField] private Camera cam;

    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (inputManager == null)
            inputManager = FindAnyObjectByType<InputManager>();
    }

    private void Update()
    {
        if (inputManager == null)
            return;

        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = inputManager.MoveVector;

        if (moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
            movement = transform.TransformDirection(movement);
            movement.y = 0;
            transform.Translate(movement, Space.World);
        }
    }

    private void HandleZoom()
    {
        float scroll = inputManager.ZoomDelta;

        if (scroll != 0f && cam != null)
        {
            float zoomChange = scroll * zoomSpeed * Time.deltaTime;

            if (cam.orthographic)
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomChange, minZoom, maxZoom);
            else
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - zoomChange, minZoom, maxZoom);
        }
    }
}