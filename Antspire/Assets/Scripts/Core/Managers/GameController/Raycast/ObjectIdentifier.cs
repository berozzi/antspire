using UnityEngine;

public class ObjectIdentifier : MonoBehaviour
{
    public event System.Action<QueenPheromoneShop> OnQueenShopHovered;
    private void Start()
    {
        RaycastManager raycastManager = FindAnyObjectByType<RaycastManager>();
        raycastManager.OnHoverEnter += IdentifyObject;  // ← SUBSCRIBE
        raycastManager.OnHoverExit += OnObjectUnhovered;
    }

    public void IdentifyObject(GameObject obj)
    {
        switch (obj) { 
            case GameObject go when go.TryGetComponent<QueenPheromoneShop>(out var shop):
                OnQueenShopHovered?.Invoke(shop);
                Debug.Log("Queen Pheromone Shop hovered");
                break;
                //case GameObject go when go.TryGetComponent<AntWorkerInfo>(out var antInfo):
                //    OnAntHovered?.Invoke(antInfo);
                //    break;
        }
    }

    private void OnObjectUnhovered(GameObject obj)
    {
        // Tutaj możesz dodać logikę dla zdarzenia unhover, jeśli potrzebujesz
    }

    // metoda IdentifyObject identyfikuje obiekt i wywołuje odpowiednie zdarzenie
    // metoda OnObjectUnhovered obsługuje zdarzenie unhover zamykając ewentualne interfejsy lub informacje
}
