using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AntBasicAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;

    [SerializeField] Transform target_A;
    [SerializeField] Transform target_B;
    Transform currentTarget;
    AntState currentState = AntState.Idle;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (target_A == null || target_B == null)
        {
            Debug.LogError("Targets are not assigned!");
            return;
        }
    }

    private void Update()
    {
        if (agent == null || target_A == null || target_B == null)
            return;
        // If the agent has reached its destination, switch targets
        switch (currentState)
        {
            case AntState.Idle:
                agent.SetDestination(target_A.position);
                currentState = AntState.Moving;
                break;
            case AntState.Moving:

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                    {
                        // Switch to the other target
                        currentTarget = (currentTarget == target_A) ? target_B : target_A;
                        agent.SetDestination(currentTarget.position);
                    }
                }
                break;
        }

    }
}

public enum AntState
{
    Moving,
    Idle
}