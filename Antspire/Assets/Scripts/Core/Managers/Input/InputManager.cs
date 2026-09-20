using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Centralizuje czytanie wejścia (klawiatura/mysz). Reszta systemów subskrybuje
/// zdarzenia zamiast czytać Keyboard.current / Mouse.current bezpośrednio.
public class InputManager : MonoBehaviour
{
    // Eventy wejścia
    public event Action OnBuildModeKeyPressed;
    public event Action OnDestroyModeKeyPressed;
    public event Action OnEscapePressed;
    public event Action OnPrimaryClickPressed;

    // Wartości osi (CameraManager je odczytuje)
    public Vector2 MoveVector { get; private set; }
    public float ZoomDelta { get; private set; }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb == null)
        {
            MoveVector = Vector2.zero;
            ZoomDelta = 0f;
            return;
        }

        if (kb.bKey.wasPressedThisFrame)
            OnBuildModeKeyPressed?.Invoke();

        if (kb.nKey.wasPressedThisFrame)
            OnDestroyModeKeyPressed?.Invoke();

        if (kb.escapeKey.wasPressedThisFrame)
            OnEscapePressed?.Invoke();

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            OnPrimaryClickPressed?.Invoke();

        MoveVector = new Vector2(
            (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f),
            (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f)
        );

        ZoomDelta = mouse != null ? mouse.scroll.ReadValue().y : 0f;
    }

    public Vector2 GetMousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }
}