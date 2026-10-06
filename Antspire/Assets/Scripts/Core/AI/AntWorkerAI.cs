using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI mrówki-workera - podstawowy komponent: stany, cykl życia i podział pętli na stany.
/// Kod jest rozłożony na trzy pliki tej samej klasy (partial):
/// <list type="bullet">
/// <item><b>AntWorkerAI.cs</b> - ten plik: referencje, stany, życie i zgon,</item>
/// <item><b>AntWorkerAI.Routines.cs</b> - szukanie rutyny oraz stany pracy i eksploracji,</item>
/// <item><b>AntWorkerAI.Transport.cs</b> - transport zasobów.</item>
/// </list>
/// Reakcja na atak to osobny komponent <see cref="AntThreatResponse"/> (stany Battling/Fleeing
/// obsługuje on, ta klasa celowo ich nie przełącza), a ruch po NavMeshie opakowuje
/// <see cref="AntMover"/>. Szukanie rutyny idzie w kolejności transport → praca → eksploracja
/// i następuje po każdym ukończonym tasku. Nie działa w pojedynkę - korzysta ze scenowych
/// <see cref="WorkplacePools"/> i <see cref="TransportBoard"/>, których referencje uzupełnia
/// w Awake, jeśli nie są podpięte.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(AntHealth), typeof(AntMover))]
public partial class AntWorkerAI : MonoBehaviour
{
    [Header("Referencje (puste pola uzupełniane w Awake)")]
    [SerializeField] private AntHealth health;
    [SerializeField] private AntMover mover;
    [SerializeField] private WorkplacePools workplacePools;
    [SerializeField] private TransportBoard transportBoard;
    [SerializeField] private Ant ant;                     // opcjonalny - lustruje carriedResources do UI/zapisu
    [SerializeField] private Transform explorationCenter; // środek eksploracji (np. gniazdo); pusta = pozycja mrówki

    [Header("Rutyny")]
    [SerializeField] private float routineRetryInterval = 0.5f; // co ile sekund ponawiać szukanie rutyny w Idle
    [SerializeField] private float defaultTaskDuration = 5f;    // task, gdy miejsce pracy nie ma zdefiniowanych

    [Header("Eksploracja")]
    [SerializeField] private float explorationRadius = 15f;
    [SerializeField] private float explorationSampleRadius = 3f;
    [SerializeField] private float explorationDwell = 2f; // odpoczynek w punkcie eksploracji

    private AntAIState state = AntAIState.Idle;

    // Aktualna rutyna - w danym momencie mrówka trzyma jedno z zajęć (zob. pliki partial).
    private Workplace claimedWorkplace;
    private WorkTaskDefinition currentTask;
    private float taskTimer;

    private float dwellTimer = -1f; // < 0 znaczy "jeszcze nie dotarł do punktu eksploracji"
    private float nextRoutineSearch;

    /// <summary>Aktualny stan mrówki - do UI, debugu i zapisu gry.</summary>
    public AntAIState State => state;

    private void Awake()
    {
        if (health == null) health = GetComponent<AntHealth>();
        if (mover == null) mover = GetComponent<AntMover>();
        if (ant == null) ant = GetComponent<Ant>();
        if (workplacePools == null) workplacePools = FindAnyObjectByType<WorkplacePools>();
        if (transportBoard == null) transportBoard = FindAnyObjectByType<TransportBoard>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
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
            // Battling i Fleeing obsługuje AntThreatResponse - tutaj rutyna jest zamrożona.
        }
    }

    /// <summary>Zmienia stan - korzysta z tego AntThreatResponse przy ataku.</summary>
    public void SetState(AntAIState next) => state = next;

    /// <summary>Wraca do taska przerwanego atakiem - wołane przez AntThreatResponse po walce/ucieczce.</summary>
    public void ResumeAfterCombat()
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

    private void HandleDied()
    {
        ReleaseHeldRoutine();
        mover.Disable();
        enabled = false;
    }
}
