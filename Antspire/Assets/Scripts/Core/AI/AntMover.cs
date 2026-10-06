using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Helper ruchu mrówki - opakowuje NavMeshAgenta, żeby logika AI nie mieszała się
/// w detale ścieżek (pathPending, remainingDistance, pathStatus). Wszystkie warunki
/// "czy dotarłem" i "czy ścieżka działa" są zdefiniowane w jednym miejscu.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AntMover : MonoBehaviour
{
    [SerializeField] private float arrivalTolerance = 0.4f;

    private NavMeshAgent agent;

    /// <summary>Czy agent właśnie stoi zamiast iść do celu.</summary>
    public bool IsStopped => agent.isStopped;

    /// <summary>Czy mrówka dotarła do ostatnio podanego celu.</summary>
    public bool HasArrived
    {
        get
        {
            if (agent.pathPending) return false;
            if (float.IsInfinity(agent.remainingDistance)) return false;

            return agent.remainingDistance <= arrivalTolerance;
        }
    }

    /// <summary>Czy ścieżka nie prowadzi do celu (cel poza NavMeshem albo tylko częściowa).</summary>
    public bool PathFailed
    {
        get
        {
            if (agent.pathPending) return false;
            if (float.IsInfinity(agent.remainingDistance)) return true;

            return agent.pathStatus != NavMeshPathStatus.PathComplete;
        }
    }

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>Idzie do celu (od razu zdejmuje blokadę postoju).</summary>
    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop() => agent.isStopped = true;

    /// <summary>Wyłącza agenta - po śmierci mrówki nie ma już czym sterować.</summary>
    public void Disable() => agent.enabled = false;

    /// <summary>Czy punkt leży w zadanym zasięgu (np. walki wręcz).</summary>
    public bool IsInReach(Vector3 point, float range)
    {
        return (point - transform.position).sqrMagnitude <= range * range;
    }

    /// <summary>Szuka miejsca na NavMeshie najbliżej podanego kandydata.</summary>
    public bool TrySamplePoint(Vector3 candidate, float sampleRadius, out Vector3 point)
    {
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}
