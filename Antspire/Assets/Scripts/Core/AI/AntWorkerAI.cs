using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Najprostsza mrówka-transport. Nie pracuje, nie szuka ani nie przyjmuje pracy i nie
/// korzysta z żadnych rejestrów tras (RouteBoard to historia). Idzie w kółko między DWOMA
/// punktami wyznaczonymi dla niej w Inspektorze:
/// <list type="bullet">
/// <item><b>originPoint</b> (A) - odbiór towaru (budynek produkujący),</item>
/// <item><b>destinationPoint</b> (B) - dostawa towaru (budynek przyjmujący input).</item>
/// </list>
/// Po dojściu do punktu mrówka próbuje zabrać (A) albo oddać (B) towar, czeka
/// <see cref="dwellOnArrival"/> sekund (interwał) i wraca do drugiego punktu - i tak w kółko.
/// Budynek, który nie przyjmuje towaru, powoduje zwrot ładunku do magazynu źródłowego.
/// Walka to osobny komponent <see cref="AntThreatResponse"/> (stany Battling/Fleeing);
/// po jej zakończeniu mrówka wraca do przerwanego odcinka marszu lub czekania.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(AntHealth), typeof(AntMover))]
public class AntWorkerAI : MonoBehaviour
{
    [Header("Punkty trasy mrówki (A → B)")]
    [SerializeField] private Transform originPoint;      // A: odbiór towaru
    [SerializeField] private Transform destinationPoint; // B: dostawa towaru

    [Header("Zachowanie")]
    [SerializeField] private float dwellOnArrival = 30f; // interwał: ile sekund czekać przy punkcie
    [SerializeField] private int carryCapacity = 5;      // ile sztuk mrówka niesie na raz
    [SerializeField] private float retryInterval = 0.5f; // co ile sekund ponawić ruch po błędzie
    [SerializeField] private float stuckMoveTimeout = 20f; // diagnostyka: uznać marsz za utknięty

    [Header("Referencje (puste = uzupełniane w Awake)")]
    [SerializeField] private AntHealth health;
    [SerializeField] private AntMover mover;
    [SerializeField] private AntThreatResponse threatResponse;
    [SerializeField] private Ant ant; // opcjonalny - lustruje carriedResources do UI/zapisu

    // Rola z panelu jest obecnie informacyjna - AI i tak tylko transportuje.
    // AntThreatResponse używa jej tylko do jednego wyjątku: defensywa nie ucieka.
    [SerializeField] private AntRole role = AntRole.Transport;

    private AntAIState state = AntAIState.Idle;

    private Transform currentPoint;  // punkt, do którego mrówka obecnie idzie
    private float dwellTimer = -1f;  // odliczanie czekania przy punkcie
    private float movingSince = -1f; // kiedy zaczął się marsz (diagnostyka utknięcia)
    private float nextTryAt;         // kolejna próba ruchu po błędzie
    private bool wasWaiting;         // czy walka przerwała czekanie przy punkcie

    // Niesiony towar.
    private ResourceDef carriedResource;
    private int carriedAmount;
    private ResourceStorage cargoOriginStorage; // skąd zabrano - tam wraca nieprzyjęty towar

    // Diagnostyka z throttlingiem (żeby nie zalać konsoli).
    private float nextBlockedLog;
    private const float blockedLogInterval = 5f;
    private float nextPathFailLog;
    private const float pathFailLogInterval = 5f;

    /// <summary>Aktualny stan mrówki - dla AntThreatResponse, UI i debugu.</summary>
    public AntAIState State => state;

    /// <summary>Rola mrówki - informacyjna; AI nie rozróżnia ról przy ruchu.</summary>
    public AntRole Role => role;

    /// <summary>Zasób, który mrówka aktualnie niesie (null, gdy nic nie niesie).</summary>
    public ResourceDef CarriedResource => carriedResource;

    /// <summary>Liczba sztuk towaru niesionego przez mrówkę (do UI i zapisu).</summary>
    public int CarriedAmount => carriedAmount;

    private void Awake()
    {
        if (health == null) health = GetComponent<AntHealth>();
        if (mover == null) mover = GetComponent<AntMover>();
        if (threatResponse == null) threatResponse = GetComponent<AntThreatResponse>();
        if (ant == null) ant = GetComponent<Ant>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied -= HandleDied;
    }

    private void OnDestroy() => ReturnCarriedToOrigin();

    private void Update()
    {
        switch (state)
        {
            case AntAIState.Idle: TickIdle(); break;
            case AntAIState.Moving: TickMoving(); break;
            case AntAIState.Working: TickWaiting(); break;
            // Battling i Fleeing obsługuje AntThreatResponse - tutaj rutyna jest zamrożona.
        }
    }

    /// <summary>Zmienia stan - AntThreatResponse wstrzykuje Battling/Fleeing.</summary>
    public void SetState(AntAIState next) => state = next;

    /// <summary>Zmienia rolę (panel) - informacyjnie; każda mrówka i tak transportuje.</summary>
    public void SetRole(AntRole newRole) => role = newRole;

    /// <summary>Powrót do przerwanej rutyny - woła AntThreatResponse po walce/ucieczce.</summary>
    public void ResumeAfterCombat()
    {
        if (wasWaiting)
        {
            state = AntAIState.Working; // dokończ czekanie przy punkcie (dwellTimer jest zamrożony)
            return;
        }

        if (currentPoint != null)
        {
            StartWalking(currentPoint);
            return;
        }

        state = AntAIState.Idle;
    }

    private void TickIdle()
    {
        if (Time.time < nextTryAt) return;

        if (currentPoint == null)
            currentPoint = originPoint != null ? originPoint : destinationPoint;

        if (currentPoint == null)
        {
            LogBlocked(); // brak punktów - wskaż je w Inspektorze mrówki
            nextTryAt = Time.time + retryInterval;
            return;
        }

        StartWalking(currentPoint);
    }

    private void TickMoving()
    {
        if (currentPoint == null)
        {
            state = AntAIState.Idle;
            return;
        }

        if (mover.HasArrived)
        {
            movingSince = -1f;
            HandleArrival();
            return;
        }

        if (mover.PathFailed)
        {
            LogPathFail();
            nextTryAt = Time.time + retryInterval;
            state = AntAIState.Idle;
            return;
        }

        WarnIfStuck();
    }

    /// <summary>Dojście do punktu: A zabiera towar, B oddaje; potem czekanie i powrót.</summary>
    private void HandleArrival()
    {
        if (currentPoint == null)
        {
            state = AntAIState.Idle;
            return;
        }

        if (currentPoint == originPoint) TakeGoods();
        else DeliverGoods();

        wasWaiting = true;
        dwellTimer = dwellOnArrival;
        state = AntAIState.Working;
        mover.Stop();
    }

    private void TickWaiting()
    {
        dwellTimer -= Time.deltaTime;
        if (dwellTimer > 0f) return;

        Transform next = currentPoint == originPoint ? destinationPoint : originPoint;
        if (next == null)
        {
            currentPoint = null;
            nextTryAt = Time.time + retryInterval;
            state = AntAIState.Idle;
            LogBlocked();
            return;
        }

        currentPoint = next;
        StartWalking(currentPoint);
    }

    private void StartWalking(Transform point)
    {
        if (point == null) return;

        wasWaiting = false;
        movingSince = -1f;
        state = AntAIState.Moving;
        mover.MoveTo(point.position);
    }

    /// <summary>Odbiór towaru w punkcie A: bierzemy każdą dostępną ilość (do carryCapacity).</summary>
    private void TakeGoods()
    {
        ResourceProducer producer = FindProducerAt(currentPoint);
        if (producer == null || producer.Storage == null) return;

        ResourceDef cargo = producer.OutputResource;
        if (cargo == null) return;

        int available = producer.Storage.GetAmount(cargo);
        if (available <= 0) return;

        int take = Mathf.Min(carryCapacity, available);
        if (!producer.Storage.TryWithdraw(cargo, take)) return;

        carriedResource = cargo;
        carriedAmount = take;
        cargoOriginStorage = producer.Storage;
        MirrorCarriedResources();

        Debug.Log($"{name}: zabiera {take}x {cargo.ResourceName} z {currentPoint.name}.", this);
    }

    /// <summary>Dostawa w punkcie B: budynek przyjmuje input i ma miejsce - inaczej towar wraca.</summary>
    private void DeliverGoods()
    {
        if (carriedResource == null || carriedAmount <= 0) return;

        ResourceProducer producer = FindProducerAt(currentPoint);
        ResourceStorage storage = producer != null ? producer.Storage : null;

        bool accepted = storage != null
            && producer.AcceptsInput(carriedResource)
            && !storage.isFull
            && storage.CanAdd(carriedResource, carriedAmount);

        if (accepted)
        {
            storage.AddResource(carriedResource, carriedAmount);
            Debug.Log($"{name}: dostarcza {carriedAmount}x {carriedResource.ResourceName} do {currentPoint.name}.", this);
            ClearCarried();
            return;
        }

        // Budynek nie przyjmuje towaru albo magazyn pełny - towar wraca do magazynu źródłowego.
        Debug.LogWarning(
            $"{name}: {currentPoint.name} nie przyjął {carriedAmount}x {carriedResource.ResourceName} " +
            "(brak inputu albo pełny magazyn) - towar wraca do magazynu źródłowego.", this);
        ReturnCarriedToOrigin();
    }

    /// <summary>Zwraca niesiony towar do magazynu, z którego pochodził (nic nie przepada).</summary>
    private void ReturnCarriedToOrigin()
    {
        if (carriedResource == null || carriedAmount <= 0) return;

        if (cargoOriginStorage != null)
            cargoOriginStorage.AddResource(carriedResource, carriedAmount);
        else
            Debug.LogWarning($"{name}: brak magazynu źródłowego - {carriedAmount}x {carriedResource.ResourceName} przepadło.", this);

        ClearCarried();
    }

    private void ClearCarried()
    {
        carriedResource = null;
        carriedAmount = 0;
        cargoOriginStorage = null;
        MirrorCarriedResources();
    }

    private void MirrorCarriedResources()
    {
        if (ant != null) ant.carriedResources = carriedAmount;
    }

    /// <summary>Znajduje producenta przy punkcie: na obiekcie, wyżej albo niżej w hierarchii.</summary>
    private static ResourceProducer FindProducerAt(Transform point)
    {
        if (point == null) return null;

        ResourceProducer producer = point.GetComponent<ResourceProducer>();
        if (producer == null) producer = point.GetComponentInParent<ResourceProducer>();
        if (producer == null) producer = point.GetComponentInChildren<ResourceProducer>();
        return producer;
    }

    private void LogBlocked()
    {
        if (Time.time < nextBlockedLog) return;

        nextBlockedLog = Time.time + blockedLogInterval;
        Debug.LogWarning(
            $"{name}: brak punktów trasy - wskaż originPoint (A) i destinationPoint (B) " +
            "w Inspektorze tej mrówki.", this);
    }

    private void LogPathFail()
    {
        if (Time.time < nextPathFailLog) return;

        nextPathFailLog = Time.time + pathFailLogInterval;
        string target = currentPoint != null ? currentPoint.name : "?";
        Debug.LogWarning(
            $"{name}: brak ścieżki do punktu {target} (poza NavMeshem albo zablokowany) - " +
            "wznawiam próby.", this);
    }

    /// <summary>Diagnostyka: marsz dłuższy niż timeout bez dotarcia = cel poza NavMeshem albo kolizja.</summary>
    private void WarnIfStuck()
    {
        if (movingSince < 0f)
        {
            movingSince = Time.time;
            return;
        }

        if (Time.time - movingSince <= stuckMoveTimeout) return;

        movingSince = Time.time; // kolejne ostrzeżenie po kolejnym timeoutie, nie co klatkę
        string target = currentPoint != null ? currentPoint.name : "?";
        Debug.LogWarning(
            $"{name}: idzie już {stuckMoveTimeout:0}s do punktu {target} bez dotarcia - " +
            "cel poza NavMeshem albo ścieżka zablokowana?", this);
        mover.LogAgentState();
    }

    private void HandleDied()
    {
        ReturnCarriedToOrigin();
        mover.Disable();
        enabled = false;
    }
}