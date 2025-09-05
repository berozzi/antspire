using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.SceneView;

public class CameraMovement : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 moveInput;
    private float zoomInput;

    [Header("Camera settings")]
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float zoomSpeed = 20f;
    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 50f;
    [SerializeField] private Camera cam;

    private void Awake()
    {
        controls = new PlayerControls();
        // Podpinamy eventy pod akcje
        controls.Camera.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Camera.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Camera.Zoom.performed += ctx => zoomInput = ctx.ReadValue<float>();
        controls.Camera.Zoom.canceled += ctx => zoomInput = 0f;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // Ruch kamery po płaszczyźnie
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
        // funkcja zooma
        Zoom();
    }

    void Zoom()
    {
        float oldSize = cam.orthographicSize;
        cam.orthographicSize -= zoomInput * zoomSpeed * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        float sizeDiff = oldSize - cam.orthographicSize;
        if (Mathf.Abs(zoomInput) > 0.001f)
        {
            Vector3 cursorWorldPos = cam.ScreenToWorldPoint(
            new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(),
                cam.nearClipPlane)
            );
            // kierunek do kursora na płaszczyźnie XY/Z
            Vector3 direction = (cursorWorldPos - cameraTransform.position).normalized;
            // przesuwasz kamerę przy zoomie
            cameraTransform.position += direction * sizeDiff * zoomSpeed * Time.deltaTime;
        }
    }
}