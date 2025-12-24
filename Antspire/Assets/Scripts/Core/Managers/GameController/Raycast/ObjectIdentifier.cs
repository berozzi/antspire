using UnityEngine;

public class ObjectIdentifier : MonoBehaviour
{
    public event System.Action<QueenPheromoneShop> OnQueenShopHovered;
    private void Start()
    {
        RaycastManager raycastManager = FindAnyObjectByType<RaycastManager>();
        raycastManager.OnHoverEnter += IdentifyObject;  // SUBSCRIBE czyli uruchomienie metody IdentifyObject przy zdarzeniu OnHoverEnter
        raycastManager.OnHoverExit += OnObjectUnhovered;
    }

    public void IdentifyObject(GameObject obj)
    {
        switch (obj) { 
            case GameObject go when go.TryGetComponent<QueenPheromoneShop>(out var shop):
                OnQueenShopHovered?.Invoke(shop);
                Debug.Log("Queen Pheromone Shop hovered");
                break;
            case GameObject go when go.TryGetComponent<Workplace>(out var workplace):
                Debug.Log("Workplace hovered"); 
                break;
            case GameObject go when go.TryGetComponent<Ant>(out var ant):
                Debug.Log("Ant hovered");
                break;
                //case GameObject go when go.TryGetComponent<AntWorkerInfo>(out var antInfo):
                //    OnAntHovered?.Invoke(antInfo);
                //    break;
        }
    }

    private void OnObjectUnhovered(GameObject obj)
    {
        
    }

    // metoda IdentifyObject identyfikuje obiekt i wywołuje odpowiednie zdarzenie
    // metoda OnObjectUnhovered obsługuje zdarzenie unhover zamykając ewentualne interfejsy lub informacje
}
