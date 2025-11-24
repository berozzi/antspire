using UnityEngine;
using UnityEngine.Events;

public class RaycastManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] LayerMask interactableLayers;
    [SerializeField] float raycastDistance = 100f;
    [SerializeField] Camera playerCamera;

    // Eventy
    public event System.Action<GameObject> OnHoverEnter;    // Najecha³ na obiekt
    public event System.Action<GameObject> OnHoverExit;     // Zjecha³ z obiektu
    public event System.Action<IClickable> OnClickableClicked;         // Klikn¹³ obiekt

    GameObject currentHoveredObject;
    GameObject previousHoveredObject;

    void Start()
    {
        interactableLayers = LayerMask.GetMask("Interactable");

        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    void Update() {}
    public void HandleMainRaycast()
    {
        var (success, hitObject) = GetRaycastHitObject();
        if (success)
        {
            currentHoveredObject = hitObject;
            // SprawdŸ czy obiekt siê zmieni³
            if (currentHoveredObject != previousHoveredObject)
            {
                if (previousHoveredObject != null)
                    OnHoverExit?.Invoke(previousHoveredObject);
                OnHoverEnter?.Invoke(currentHoveredObject);
                previousHoveredObject = currentHoveredObject;
            }
        }
        else
        {
            // Nie ma obiektu pod kursorem
            if (currentHoveredObject != null)
            {
                OnHoverExit?.Invoke(currentHoveredObject);
                currentHoveredObject = null;
                previousHoveredObject = null;
            }
        }
        
    }

    private (bool, GameObject) GetRaycastHitObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
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
            Debug.Log("RaycastManager: Click detected on " + currentHoveredObject.name);
            var clickable = currentHoveredObject.GetComponent<IClickable>();
            if (clickable != null)
                OnClickableClicked?.Invoke(clickable);
        }
    }
}
