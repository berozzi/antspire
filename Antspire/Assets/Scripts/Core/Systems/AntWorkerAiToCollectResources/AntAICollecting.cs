using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntAICollecting : MonoBehaviour
{
    public Transform targetResource;
    public Transform homeBase;
    public float collectionRange = 3f;
    public float collectionTime = 2.0f;
    private NavMeshAgent agent;
    private bool isCollecting = false;
    private bool hasResource = false;
    private float distanceToResource;

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
    }

    // Update is called once per frame
    void Update()
    {
        if(isCollecting || hasResource)
        {
            hasResource = false;
            return;
        }
        
        if (targetResource != null)
        {
            agent.SetDestination(targetResource.position);
            distanceToResource = Vector3.Distance(transform.position, targetResource.position);

            if (distanceToResource <= collectionRange && !isCollecting)
            {
                StartCoroutine(CollectResource());
            }
        }
    }

    private System.Collections.IEnumerator CollectResource()
    {
        isCollecting = true;
        agent.isStopped = true;
        // Simulate collection time
        yield return new WaitForSeconds(collectionTime);
        // After collecting, head back to home base
        hasResource = true;
        agent.isStopped = false;
        agent.SetDestination(homeBase.position);
        // Simulate dropping off resources at home base
        yield return new WaitUntil(() => Vector3.Distance(transform.position, homeBase.position) <= collectionRange);
        // After dropping off, go back to the resource
        if (targetResource != null)
        {
            agent.SetDestination(targetResource.position);
        }
        isCollecting = false;
    }
}
