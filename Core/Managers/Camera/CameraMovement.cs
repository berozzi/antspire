using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera settings")]
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float zoomSpeed = 20f;
    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 50f;
    [SerializeField] private Camera cam;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = new Vector2(
            Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
            Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0
        );

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
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0f && cam != null)
        {
            float zoomChange = scroll * zoomSpeed * Time.deltaTime;

            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomChange, minZoom, maxZoom);
            }
            else
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - zoomChange, minZoom, maxZoom);
            }
        }
    }
}