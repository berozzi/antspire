using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RaycastManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] LayerMask interactableLayers;
    [SerializeField] float raycastDistance = 100f;
    [SerializeField] Camera playerCamera;

    // Eventy
    public event System.Action<GameObject> OnHoverEnter;    // Najecha� na obiekt
    public event System.Action<GameObject> OnHoverExit;     // Zjecha� z obiektu
    public event System.Action<IClickable, ClickableData> OnClickableClicked;         // Klikn�� obiekt

    GameObject currentHoveredObject;
    GameObject previousHoveredObject;

    void Start()
    {
        interactableLayers = LayerMask.GetMask("Interactable");

        if (playerCamera == null)
            playerCamera = Camera.main;

        
    }
    public void HandleMainRaycast()
    {
        var (success, hitObject) = GetRaycastHitObject();

        GameObject objectUnderCursor = success ? hitObject : null;

        // Je�li obiekt pod kursorem si� zmieni� (w stosunku do obecnego hoverowanego)
        if (objectUnderCursor != currentHoveredObject)
        {
            // Je�li by� poprzedni obiekt, to wywo�aj Exit
            if (currentHoveredObject != null)
            {
                
                OnHoverExit?.Invoke(currentHoveredObject);
            }

            // Je�li teraz jest obiekt, to wywo�aj Enter
            if (objectUnderCursor != null)
            {
                
                OnHoverEnter?.Invoke(objectUnderCursor);
            }

            // Zaktualizuj obecny obiekt
            currentHoveredObject = objectUnderCursor;
        }
    }

    private (bool, GameObject) GetRaycastHitObject()
    {
        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayers))
        {
            return (true, hit.collider.gameObject);
        }
        return (false, null);
    }

    public void HandleRaycastInput()
    {
        if (currentHoveredObject != null)
        {
            //Debug.Log("RaycastManager: Click detected on " + currentHoveredObject.name);
            var clickedObject = currentHoveredObject.GetComponent<IClickable>();
            if (clickedObject != null)
            {
                ClickableData data = clickedObject.GetClickableData();
                OnClickableClicked?.Invoke(clickedObject, data);
            }
            // if object is clickable, invoke the event
            // and on that line change ObjectInfoPanel to show info about clickedObject - name, type, description
            
        }
    }

    
}
