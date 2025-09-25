using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class AntAICollecting : MonoBehaviour
{
    /*
     Podsumowanie kodu:
    Kod dzia³a perfekcyjnie. Mrówka porusza siê do zasobu, zbiera go po dotarciu, a nastêpnie wraca do bazy. Wystarczy ustawiæ jedynie drop off time.
    Problemy: rotacja - mrówka krêci siê w ka¿d¹ stronê a nie w kierunku w którym idzie (funkcja HeadDummy jest do poprawy)
    ¯eby mrówka nie wchodzi³a w œciany podczas poruszania siê, dostosujemy te œciany aby mia³y swoj¹ odleg³oœæ/wielkoœæ w sensie ogólnie dystants do którego nie mo¿e podejœæ mrówka.
     */
    [SerializeField] Transform targetResource;
    [SerializeField] Transform homeBase;
    //[SerializeField] Transform headDummy;
    Quaternion initialRotation;
    float collectionRange = 2f;
    float collectionTime = 2.0f;
    NavMeshAgent agent;
    bool isCollecting = false;
    bool hasResource = false;
    float distanceToTarget;

    private void Awake()
    {
        if (targetResource == null)
        {
            Debug.LogError("Target Resource not assigned in the inspector.");
        }
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
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasResource)
        {
            ReturnToBase();
            //HeadDummyMovement(homeBase);
        }
        else
        {
            MoveToResource();
            //HeadDummyMovement(targetResource);
        }
        // Always handle rotation to face movement direction but for now it isn't working properly
        // Also HeadDummyMovement is overriding it or simply don't work well with it
        // Future me: fix Z rotation issue, maybe use another approach and don't set X and Y rotation to constant values 
        HandleRotation();
    }
    //private void HeadDummyMovement(Transform target)
    //{
    //    if (target != null)
    //    {
    //        Vector3 dir = (target.position - headDummy.position).normalized;
    //        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
    //        headDummy.rotation = Quaternion.Slerp(headDummy.rotation, lookRot, Time.deltaTime * 8f);
    //    }
    //}
    private void HandleRotation()
    {
        Vector3 velocity = agent.velocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            // kierunek w poziomie
            Vector3 flatDir = new Vector3(velocity.x, 0f, velocity.z).normalized;

            if (flatDir.sqrMagnitude > 0.001f)
            {
                float targetAngle = Mathf.Atan2(flatDir.z, flatDir.x) * Mathf.Rad2Deg;

                // X zostaje -90 (¿eby mrówka sta³a na ziemi), Y zostaje taki jak w initialRotation,
                // Z obraca siê zgodnie z kierunkiem ruchu
                transform.rotation = Quaternion.Euler(
                    initialRotation.eulerAngles.x,
                    initialRotation.eulerAngles.y,
                    targetAngle
                );
            }
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }
    private void MoveToResource()
    {
        if (targetResource == null || isCollecting) return;
        agent.SetDestination(targetResource.position);
        distanceToTarget = Vector3.Distance(transform.position, targetResource.position);
        Debug.Log($"Moving to resource, distance: {distanceToTarget}");
        if (distanceToTarget <= collectionRange)
        {
            Debug.Log("Reached resource, starting collection...");
            StartCoroutine(CollectResource());
        }
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
    private System.Collections.IEnumerator CollectResource()
    {
        Debug.Log("Collecting resource...");
        isCollecting = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(collectionTime);
        hasResource = true;
        isCollecting = false;
        agent.isStopped = false;
    }
}