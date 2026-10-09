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

    [Header("Docelowy punkt na NavMeshie")]
    [SerializeField]
    [Tooltip("MoveTo próbuje przyciągnąć cel do NavMesha w tym promieniu. Punkty " +
             "tras/workplace bywają lekko pod lub nad powierzchnią (testowe meshe) - " +
             "SetDestination na punkt poza NavMeshem daje ścieżkę częściową albo " +
             "nierozliczoną (remaining=Infinity), co wygląda jak 'brak ścieżki'.")]
    private float destinationSnapRadius = 2f;

    [Header("Ochrona przed fałszywym PathFailed")]
    [SerializeField]
    [Tooltip("Po MoveTo tyle sekund PathFailed zwraca false - ścieżka dopiero co jest " +
             "wyznaczana i przez chwilę remainingDistance bywa niepoliczone.")]
    private float pathGracePeriod = 0.3f;

    [Header("Diagnostyka (tymczasowa)")]
    [SerializeField] private bool logAgentState = false; // co 2 s loguje stan agenta w ruchu
    [SerializeField] private float stateLogInterval = 2f;

    private NavMeshAgent agent;
    private float nextStateLog;
    private float lastMoveToAt = float.NegativeInfinity; // kiedy ostatnio wyznaczono cel (okno grace period)

    // Diagnostyka pętli MoveTo: ostatni cel, czas ostatniego loga i licznik
    // powtórzeń tego samego celu z rzędu (wykrywa "wyznacza ten sam cel w kółko").
    private AntWorkerAI worker;
    private Vector3 lastMoveTo;
    private float lastMoveToLogAt = float.NegativeInfinity;
    private int repeatedMoveCalls = 1;

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

            // Zaraz po MoveTo ścieżka jeszcze się wyznacza i remainingDistance bywa
            // niepoliczone (Infinity) - bez tego okna mrówka przerywałaby transport
            // w klatce po wysłaniu celu ("brak ścieżki" na samym starcie).
            if (Time.time - lastMoveToAt < pathGracePeriod) return false;

            if (float.IsInfinity(agent.remainingDistance)) return true;

            return agent.pathStatus != NavMeshPathStatus.PathComplete;
        }
    }

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        worker = GetComponent<AntWorkerAI>(); // nil w potencjalnych nietypowych prefabach - logi to uwzględniają
    }

    private void Update()
    {
        if (logAgentState && !agent.isStopped && Time.time >= nextStateLog)
        {
            nextStateLog = Time.time + stateLogInterval;
            LogAgentState();
        }
    }

    /// <summary>
    /// Wyciąg pełnego stanu agenta - do badania, dlaczego mrówka nie dochodzi do celu.
    /// Pokazuje: cel vs pozycję, remainingDistance, status ścieżki, prędkość i kolizje.
    /// </summary>
    public void LogAgentState()
    {
        bool onNavMesh = agent.isOnNavMesh;
        Vector3 destination = agent.destination;
        float remaining = agent.remainingDistance;
        float speed = agent.velocity.magnitude;

        // Ile agent realnie przebył od ostatniego loga - odróżnia "stoi w miejscu"
        // od "idzie, ale krąży" od "dociera, ale tolerancja za mała".
        Vector3 toDestination = destination - transform.position;
        float straightLine = toDestination.magnitude;

        Debug.Log(
            $"{name}: [agent] stan={(worker != null ? worker.State.ToString() : "?")}, " +
            $"naNavMesh={onNavMesh}, " +
            $"pozycja={transform.position.ToString("F1")}, " +
            $"cel={destination.ToString("F1")}, " +
            $"prosto={straightLine:0.00}, remaining={remaining:0.00} " +
            $"(tolerancja {arrivalTolerance}), path={agent.pathStatus}, " +
            $"pending={agent.pathPending}, prędkość={speed:0.00}, " +
            $"isStopped={agent.isStopped}, " +
            $"nextPos={agent.nextPosition.ToString("F1")}", this);
    }

    /// <summary>
    /// Idzie do celu (od razu zdejmuje blokadę postoju). Cel jest najpierw przyciągany
    /// do NavMesha w promieniu destinationSnapRadius - surowe punkty transformów
    /// (trasa, workplace) bywają pod/nad powierzchnią i wtedy SetDestination daje
    /// ścieżkę częściową albo niepoliczoną.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        lastMoveToAt = Time.time; // start grace period dla PathFailed

        // Dwa przypadki, w których SetDestination nic nie zmienia i mrówka stoi "wcale":
        // agent nie stoi na NavMeshie albo cel nie został przyjęty. Bez tego loga
        // problem wygląda jakby AI w ogóle nie próbował ruszyć.
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                $"{name}: MoveTo - agent nie stoi na NavMeshie, cel {destination.ToString("F1")} " +
                "zostanie odrzucony. Sprawdź pozycję startową mrówki i bake NavMesha.", this);
        }

        Vector3 finalDestination = SnapToNavMesh(destination);
        agent.isStopped = false;

        if (!agent.SetDestination(finalDestination))
        {
            Debug.LogWarning(
                $"{name}: SetDestination odrzucił cel {finalDestination.ToString("F1")} " +
                $"(surowy cel {destination.ToString("F1")}) - agent poza NavMeshem albo cel nieosiągalny.", this);
        }

        LogRepeatedMove(finalDestination);
    }

    /// <summary>
    /// Przyciąga cel do najbliższego punktu na NavMeshie (jeśli jest w promieniu
    /// destinationSnapRadius). Poza promieniem cel zostaje bez zmian - to cel
    /// rzeczywiście odległy, a nie "lekko pod powierzchnią".
    /// </summary>
    private Vector3 SnapToNavMesh(Vector3 destination)
    {
        if (destinationSnapRadius <= 0f) return destination;
        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, destinationSnapRadius, NavMesh.AllAreas))
            return destination;

        return hit.position;
    }

    /// <summary>
    /// Diagnostyka pętli: jeśli ten sam cel jest wyznaczany wielokrotnie z rzędu,
    /// loguje to z liczbą powtórzeń. Normalny marsz zmienia cel albo kończy się
    /// stanem innym niż Moving - powtarzanie tego samego celu przy remaining=0
    /// oznacza, że pętla AI nie przechodzi dalej i SetDestination resetuje
    /// pathPending w nieskończoność.
    /// </summary>
    private void LogRepeatedMove(Vector3 destination)
    {
        if (!logAgentState) return;

        bool sameDestination = destination == lastMoveTo;
        lastMoveTo = destination;
        if (!sameDestination)
        {
            repeatedMoveCalls = 1;
            lastMoveToLogAt = Time.time;
            return;
        }

        repeatedMoveCalls++;

        // Log co sekundę, nie co klatkę - inaczej konsola zalana setkami linii.
        if (Time.time - lastMoveToLogAt < 1f) return;
        lastMoveToLogAt = Time.time;

        string workerState = worker != null ? worker.State.ToString() : "?";
        Debug.LogWarning(
            $"{name}: [MoveTo] ten sam cel {destination.ToString("F1")} wyznaczony " +
            $"{repeatedMoveCalls}razy pod rząd - pętla AI nie przechodzi dalej " +
            $"(stan {workerState}, remaining={agent.remainingDistance:0.00}). " +
            "Sprawdź stan mrówki i ostatnie logi przed tą wiadomością.", this);
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
