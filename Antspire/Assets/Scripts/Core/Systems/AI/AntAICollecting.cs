using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class AntAICollecting : MonoBehaviour
{
    //Transform targetResource;
    [SerializeField] Transform homeBase;
    Quaternion initialRotation;
    float collectionRange = 0.1f;
    float collectionTime = 2.0f;
    NavMeshAgent agent;
    bool isCollecting = false;
    bool hasResource = false;
    float distanceToTarget;
    int inventoryCapacity = 5;

    [Header("Resource Finding")]
    public float searchRefreshRate = 2f; // Co ile sekund szuka nowych zasobów

    // Nowe zmienne które musisz dodaæ:
    private ResourceData currentTargetResource;
    private Vector3 targetResourcePosition;


    private void Awake()
    {
        if (homeBase == null)
        {
            Debug.LogError("Home Base not assigned in the inspector.");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // We will handle rotation manually
        //initialRotation = transform.rotation;
        FindNearestResource();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasResource && !isCollecting)
        {
            MoveToResource();
        }
        else if (hasResource)
        {
            ReturnToBase();
        }
    }
    // znajdŸ najbli¿szy zasób z GameSave i ustaw go jako targetResource
    private void FindNearestResource()
    {
        GameSave gameSave = GameSave.Instance;

        if (gameSave.resources == null || gameSave.resources.Count == 0)
        {
            Debug.Log("Brak zasobów w zapisie gry");
            return;
        }

        Vector3 currentPos = transform.position;
        ResourceData nearestResource = null;
        float shortestDistance = float.MaxValue;

        // Szukaj najbli¿szego DOSTÊPNEGO zasobu
        foreach (var resource in gameSave.resources)
        {
            if (!resource.isAvailable) continue; // Pomijaj ju¿ zebrane zasoby

            // Konwertuj zapisane x,y na Vector3
            Vector3 resourcePos = new Vector3(resource.x, resource.y, transform.position.z);
            float distance = Vector3.Distance(currentPos, resourcePos);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestResource = resource;
            }
        }

        if (nearestResource != null)
        {
            currentTargetResource = nearestResource;
            targetResourcePosition = new Vector3(nearestResource.x, nearestResource.y, transform.position.z);
            // tutaj trzeba oznaczyc ten obiekt jako niestniejacy aby mrowka szukala innego
            Debug.Log($"Mrówka znalaz³a zasób: {nearestResource.resourceName} w odleg³oœci: {shortestDistance:F1}");
        }
        else
        {
            Debug.Log("Brak dostêpnych zasobów");
            currentTargetResource = null;
        }
    }
    private void MoveToResource()
    {
        if (currentTargetResource == null) return;
        if (!currentTargetResource.isAvailable)
        {
            FindNearestResource(); // ZnajdŸ nowy zasób jeœli ten zosta³ zebrany
            return;
        }

        if (!isCollecting)
        {
            // Ustaw cel nawigacji
            agent.SetDestination(targetResourcePosition);

            float distanceToTarget = Vector3.Distance(transform.position, targetResourcePosition);

            if (distanceToTarget <= collectionRange)
            {
                Debug.Log("Mrówka dotar³a do zasobu, zaczyna zbieraæ...");
                StartCoroutine(CollectResource());
            }
        }
    }

    // Kolekcjonowanie zasobu
    private IEnumerator CollectResource()
    {
        if (currentTargetResource == null) yield break;

        isCollecting = true;
        agent.isStopped = true; // Zatrzymaj mrówkê podczas zbierania

        Debug.Log($"Zbieranie: {currentTargetResource.resourceName}...");

        // Czekaj przez czas zbierania
        yield return new WaitForSeconds(collectionTime);

        // Po zebraniu:
        if (currentTargetResource != null)
        {
            //logika co siê dzieje po zebraniu
            // Np. dodaj punkty, zasoby itp.
            Debug.Log($"Zebrano: {currentTargetResource.resourceName}!");
            // Oznacz zasób jako zebrany (ju¿ niedostêpny)
            currentTargetResource.isAvailable = false;
            // Wyczyœæ cel
            currentTargetResource = null;
        }

        agent.isStopped = false; // Wznów ruch
        isCollecting = false;

        // Szukaj nastêpnego zasobu
        FindNearestResource();
        //inventoryCapacity++;
        //if (inventoryCapacity >= 5)
        //{
        //    hasResource = true;
        //    Debug.Log("Inventory full, returning to base...");
        //    ReturnToBase();
        //    inventoryCapacity = 0;
        //}
    }

    // METODA POMOCNICZA - Wywo³aj j¹ gdy dodajesz nowe zasoby do gry
    public void RefreshResources()
    {
        FindNearestResource();
    }
    private void ReturnToBase()
    {
        if (homeBase == null) return;
        Debug.Log("Returning to base...");
        agent.SetDestination(homeBase.position);
        distanceToTarget = Vector3.Distance(transform.position, homeBase.position);
        if (distanceToTarget <= collectionRange)
        {
            Debug.Log("Reached base, dropping off resource...");
            hasResource = false;
            // Optionally, you can add logic here to "drop off" the resource
        }
    }
    //private System.Collections.IEnumerator CollectResource()
    //{
    //    Debug.Log("Collecting resource...");
    //    isCollecting = true;
    //    agent.isStopped = true;

    //    yield return new WaitForSeconds(collectionTime);

    //    hasResource = true;
    //    isCollecting = false;
    //    agent.isStopped = false;
        
    //}
}