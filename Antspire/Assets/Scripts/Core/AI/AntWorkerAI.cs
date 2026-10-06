using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI mrówki-workera. Szuka rutyny w kolejności transport → praca → eksploracja,
/// wykonuje ją, a po każdym ukończonym tasku natychmiast szuka kolejnej.
/// Reaguje na atak: walczy z napastnikiem, a gdy ten jest silniejszy - instynktownie ucieka.
/// Po zakończonej walce albo ucieczce wraca do przerwanego taska.
/// Nie działa w pojedynkę - korzysta ze scenowych <see cref="WorkplacePools"/> i
/// <see cref="TransportBoard"/>, których referencje uzupełnia w Awake, jeśli nie są podpięte.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AntWorkerAI : MonoBehaviour
{
    [Header("Referencje (puste pola uzupełniane w Awake)")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AntHealth health;
    [SerializeField] private WorkplacePools workplacePools;
    [SerializeField] private TransportBoard transportBoard;
    [SerializeField] private Ant ant;                     // opcjonalny - lustruje carriedResources do UI/zapisu
    [SerializeField] private Transform explorationCenter; // środek eksploracji (np. gniazdo); pusta = pozycja mrówki

    [Header("Rutyny")]
    [SerializeField] private float routineRetryInterval = 0.5f; // co ile sekund ponawiać szukanie rutyny w Idle
    [SerializeField] private float defaultTaskDuration = 5f;    // task, gdy miejsce pracy nie ma zdefiniowanych
    [SerializeField] private float arrivalTolerance = 0.4f;     // dystans, przy którym uznajemy dojście do celu

    [Header("Eksploracja")]
    [SerializeField] private float explorationRadius = 15f;
    [SerializeField] private float explorationSampleRadius = 3f;
    [SerializeField] private float explorationDwell = 2f; // odpoczynek w punkcie eksploracji

    [Header("Walka")]
    [SerializeField] private float strength = 5f;
    [SerializeField] private float meleeDamage = 2f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chasePathInterval = 0.5f; // jak często odświeżać ścieżkę w pogoni
    [SerializeField] private float fleeDuration = 5f;
    [SerializeField] private float fleeDistance = 12f;
    [SerializeField] private float fleeSampleRadius = 6f;

    private AntAIState state = AntAIState.Idle;

    // Rutyna - w danym momencie mrówka trzyma dokładnie jedno z tych trzech "zajęć".
    private Workplace claimedWorkplace;
    private WorkTaskDefinition currentTask;
    private float taskTimer;

    private TransportRequest activeTransport;
    private bool isCarrying;

    private float dwellTimer = -1f; // < 0 znaczy "jeszcze nie dotarł do punktu eksploracji"
    private float nextRoutineSearch;

    // Walka / ucieczka.
    private EnemyUnit combatTarget;
    private float attackTimer;
    private float nextChasePath;
    private float fleeEndsAt;

    /// <summary>Aktualny stan mrówki - do UI, debugu i zapisu gry.</summary>
    public AntAIState State => state;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<AntHealth>();
        if (ant == null) ant = GetComponent<Ant>();
        if (workplacePools == null) workplacePools = FindAnyObjectByType<WorkplacePools>();
        if (transportBoard == null) transportBoard = FindAnyObjectByType<TransportBoard>();
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    private void OnDestroy()
    {
        // Nie zostawiamy zajętych etatów i zleceń po zniszczonej mrówce.
        ReleaseHeldRoutine();
    }

    private void Update()
    {
        switch (state)
        {
            case AntAIState.Idle: TickIdle(); break;
            case AntAIState.Moving: TickMoving(); break;
            case AntAIState.Working: TickWorking(); break;
            case AntAIState.Exploring: TickExploring(); break;
            case AntAIState.Battling: TickBattling(); break;
            case AntAIState.Fleeing: TickFleeing(); break;
        }
    }

    // ----------------------------------------------------------- szukanie rutyny

    /// <summary>Szukanie kolejnej rutyny - wołane starcie, po każdym ukończonym tasku i po walce bez czegoś do wznowienia.</summary>
    private void SearchRoutine()
    {
        ReleaseHeldRoutine();

        if (TryStartTransport()) return;
        if (TryStartWork()) return;
        if (TryStartExploration()) return;

        state = AntAIState.Idle;
        nextRoutineSearch = Time.time + routineRetryInterval;
    }

    private bool TryStartTransport()
    {
        if (transportBoard == null) return false;
        if (!transportBoard.TryTakeNext(out TransportRequest request)) return false;

        activeTransport = request;
        isCarrying = false;
        ResumeTransport();
        return true;
    }

    private bool TryStartWork()
    {
        if (workplacePools == null) return false;
        if (!workplacePools.TryClaimNext(out Workplace workplace)) return false;

        claimedWorkplace = workplace;
        currentTask = workplace.PickRandomTask();
        ResumeWork();
        return true;
    }

    private bool TryStartExploration()
    {
        if (!TryPickExplorePoint(out Vector3 point)) return false;

        dwellTimer = -1f;
        state = AntAIState.Exploring;
        MoveTo(point);
        return true;
    }

    /// <summary>Zwalnia wszystko, co mrówka trzymała - używane przed nową rutyną, przy zgonie i zniszczeniu.</summary>
    private void ReleaseHeldRoutine()
    {
        ReleaseWorkplace();
        DropCarriedTransport();
    }

    private void ReleaseWorkplace()
    {
        if (claimedWorkplace == null) return;

        claimedWorkplace.Release();
        claimedWorkplace = null;
        currentTask = null;
    }

    private void DropCarriedTransport()
    {
        if (activeTransport == null) return;

        // Zlecenie wraca do kolejki - inna mrówka dowiezie ładunek.
        if (transportBoard != null) transportBoard.Release(activeTransport);

        activeTransport = null;
        isCarrying = false;
        MirrorCarried();
    }

    /// <summary>Nic do roboty albo cel nieosiągalny - czekamy i ponawiamy szukanie rutyny.</summary>
    private void CancelRoutine()
    {
        ReleaseHeldRoutine();
        StopMoving();
        state = AntAIState.Idle;
        nextRoutineSearch = Time.time + routineRetryInterval;
    }

    // ----------------------------------------------------------- stany rutyny

    private void TickIdle()
    {
        if (Time.time < nextRoutineSearch) return;
        SearchRoutine();
    }

    private void TickMoving()
    {
        if (HasArrived())
        {
            HandleRoutineArrival();
            return;
        }

        if (PathFailed()) CancelRoutine();
    }

    private void HandleRoutineArrival()
    {
        if (activeTransport != null)
        {
            HandleTransportArrival();
            return;
        }

        if (claimedWorkplace != null)
        {
            BeginWork();
            return;
        }

        SearchRoutine(); // brak celu - bezpieczne wyjście do szukania rutyny od nowa
    }

    private void HandleTransportArrival()
    {
        if (!isCarrying)
        {
            PickUpTransport();
            return;
        }

        activeTransport.Deliver();
        transportBoard.Complete(activeTransport);
        activeTransport = null;
        isCarrying = false;
        MirrorCarried();
        SearchRoutine();
    }

    private void PickUpTransport()
    {
        if (!activeTransport.TryPickUp())
        {
            // źródło nie ma już zasobu - zlecenie jest do wyrzucenia.
            transportBoard.Cancel(activeTransport);
            activeTransport = null;
            SearchRoutine();
            return;
        }

        isCarrying = true;
        MirrorCarried();
        MoveTo(activeTransport.Target.transform.position);
    }

    private void BeginWork()
    {
        taskTimer = currentTask != null ? currentTask.DurationSeconds : defaultTaskDuration;
        state = AntAIState.Working;
        StopMoving();
    }

    private void TickWorking()
    {
        taskTimer -= Time.deltaTime;
        if (taskTimer > 0f) return;

        // Efekt ukończonego taska (np. cykl produkcji budynku) - miejsce na rozszerzenie tutaj.
        SearchRoutine(); // task skończony -> zwalniamy etat i od razu szukamy następnej rutyny
    }

    private void TickExploring()
    {
        if (dwellTimer < 0f)
        {
            if (PathFailed())
            {
                CancelRoutine();
                return;
            }
            if (!HasArrived()) return;

            StopMoving();
            dwellTimer = explorationDwell;
        }

        dwellTimer -= Time.deltaTime;
        if (dwellTimer > 0f) return;

        dwellTimer = -1f;
        SearchRoutine();
    }

    // ----------------------------------------------------------- reakcja na atak

    private void HandleDamaged(EnemyUnit attacker)
    {
        if (attacker == null || attacker.IsDead) return;
        if (state == AntAIState.Battling || state == AntAIState.Fleeing) return;

        // Instynkt: większa siła napastnika = ucieczka, w przeciwnym razie kontraatak.
        if (attacker.Strength > strength) StartFleeing(attacker);
        else StartBattling(attacker);
    }

    private void HandleDied()
    {
        DropCarriedTransport();
        ReleaseWorkplace();
        StopMoving();
        if (agent != null) agent.enabled = false;
        enabled = false;
    }

    private void StartBattling(EnemyUnit attacker)
    {
        combatTarget = attacker;
        attackTimer = 0f;
        nextChasePath = 0f;
        state = AntAIState.Battling;
    }

    private void TickBattling()
    {
        if (combatTarget == null || combatTarget.IsDead)
        {
            EndCombat();
            return;
        }

        if (IsInMeleeRange(combatTarget))
        {
            if (!agent.isStopped) StopMoving();

            attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
            if (attackTimer > 0f) return;

            attackTimer = attackInterval;
            combatTarget.TakeDamage(meleeDamage);
            return;
        }

        ChaseCombatTarget();
    }

    private void ChaseCombatTarget()
    {
        if (Time.time < nextChasePath) return;

        nextChasePath = Time.time + chasePathInterval;
        MoveTo(combatTarget.transform.position);
    }

    private void StartFleeing(EnemyUnit attacker)
    {
        combatTarget = attacker;
        fleeEndsAt = Time.time + fleeDuration;
        state = AntAIState.Fleeing;
        MoveTo(GetFleePoint(attacker));
    }

    private void TickFleeing()
    {
        bool safe = combatTarget == null || combatTarget.IsDead || Time.time >= fleeEndsAt || HasArrived();
        if (safe || PathFailed())
        {
            EndCombat();
            return;
        }
    }

    /// <summary>Koniec przerwy - mrówka wraca do taska, który wykonywała przed atakiem.</summary>
    private void EndCombat()
    {
        combatTarget = null;
        ResumeRoutineAfterCombat();
    }

    private void ResumeRoutineAfterCombat()
    {
        if (activeTransport != null)
        {
            ResumeTransport();
            return;
        }

        if (claimedWorkplace != null)
        {
            ResumeWork();
            return;
        }

        SearchRoutine();
    }

    private void ResumeTransport()
    {
        state = AntAIState.Moving;
        Vector3 destination = isCarrying
            ? activeTransport.Target.transform.position
            : activeTransport.Source.transform.position;
        MoveTo(destination);
    }

    private void ResumeWork()
    {
        state = AntAIState.Moving;
        MoveTo(claimedWorkplace.transform.position);
    }

    // ----------------------------------------------------------- helpery ruchu i celu

    private void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    private void StopMoving()
    {
        agent.isStopped = true;
    }

    private bool HasArrived()
    {
        if (agent.pathPending) return false;
        if (float.IsInfinity(agent.remainingDistance)) return false;

        return agent.remainingDistance <= arrivalTolerance;
    }

    /// <summary>Ścieżka nie prowadzi do celu (cel poza NavMeshem albo tylko częściowa ścieżka).</summary>
    private bool PathFailed()
    {
        if (agent.pathPending) return false;
        if (float.IsInfinity(agent.remainingDistance)) return true;

        return agent.pathStatus != NavMeshPathStatus.PathComplete;
    }

    private bool IsInMeleeRange(EnemyUnit target)
    {
        float sqrDistance = (target.transform.position - transform.position).sqrMagnitude;
        return sqrDistance <= attackRange * attackRange;
    }

    /// <summary>Punkt ucieczki w linii prostej od napastnika, próbkowany na NavMeshie.</summary>
    private Vector3 GetFleePoint(EnemyUnit attacker)
    {
        Vector3 away = transform.position - attacker.transform.position;
        if (away.sqrMagnitude < 0.0001f) away = Vector3.forward;
        else away.Normalize();

        Vector3 target = transform.position + away * fleeDistance;
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, fleeSampleRadius, NavMesh.AllAreas))
            return hit.position;

        return target; // bez gwarancji na NavMeshu - obsłuży to PathFailed
    }

    private bool TryPickExplorePoint(out Vector3 point)
    {
        Vector3 origin = explorationCenter != null ? explorationCenter.position : transform.position;
        Vector3 candidate = origin + Random.insideUnitSphere * explorationRadius;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, explorationSampleRadius, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    /// <summary>Lustruje ilość niesionych zasobów do komponentu Ant (UI/zapis gry).</summary>
    private void MirrorCarried()
    {
        if (ant == null) return;

        ant.carriedResources = isCarrying && activeTransport != null ? activeTransport.Amount : 0;
    }
}
