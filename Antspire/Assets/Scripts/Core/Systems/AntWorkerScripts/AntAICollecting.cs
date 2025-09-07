using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntAICollecting : MonoBehaviour
{
    public Transform targetResource;
    public Transform homeBase;
    Quaternion initialRotation;
    float collectionRange = 4f;
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
        }
        else
        {
            MoveToResource();
        }
    }
    void LateUpdate()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Keep your X rotation (-90) but update Z based on movement
            transform.rotation = Quaternion.Euler(-90, 0, targetAngle);
        }
        else
        {
            transform.rotation = initialRotation; // Return to (-90, 0, 89)
        }
    }
    private void MoveToResource()
    {
        if (targetResource == null || isCollecting) return;
        agent.SetDestination(targetResource.position);
        distanceToTarget = Vector3.Distance(transform.position, targetResource.position);
        LookAt(targetResource.position);
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
        LookAt(homeBase.position);
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

    void LookAt(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}