using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dyrektor zagrożeń mrowiska - źródło akcji DANGER.
/// Pilnuje strefy wokół mrowiska (<see cref="anthillCenter"/> + <see cref="dangerRadius"/>)
/// i informuje eventami, gdy wróg (<see cref="EnemyUnit"/>) do niej wejdzie albo z niej wyjdzie:
/// <list type="bullet">
/// <item><see cref="OnDangerStarted"/> / <see cref="OnDangerCleared"/> - początek i koniec akcji DANGER,</item>
/// <item><see cref="OnThreatAppeared"/> / <see cref="OnThreatRemoved"/> - konkretne zagrożenie.</item>
/// </list>
/// Wrogowie rejestrują się sami (<see cref="EnemyUnit.OnEnable"/> → <see cref="Register"/>),
/// a stan strefy odświeżamy co <see cref="pollInterval"/> sekundy, żeby wyłapać też śmierć
/// wroga i jego wejście do strefy po spacerze. Mrówki reagują na te eventy według roli:
/// defensywa atakuje, eksploracja ucieka albo walczy (zob. <see cref="AntWorkerAI"/>).
/// Referencję podkładasz w Inspektorze albo zostawiasz pustą - scena zostanie przeszukana.
/// </summary>
public class DangerDirector : MonoBehaviour
{
    [Header("Strefa mrowiska")]
    [SerializeField] private Transform anthillCenter; // pusta = pozycja tego obiektu
    [SerializeField] private float dangerRadius = 25f;

    [Header("Odświeżanie stanu strefy")]
    [SerializeField] private float pollInterval = 0.5f;
    [SerializeField] private bool enableLogs = true;

    private readonly List<EnemyUnit> registered = new List<EnemyUnit>();
    private readonly List<EnemyUnit> threats = new List<EnemyUnit>();
    private readonly List<EnemyUnit> pollBuffer = new List<EnemyUnit>();

    private float nextPoll;

    /// <summary>Akcja DANGER - pierwszy wróg wszedł do strefy mrowiska.</summary>
    public event Action OnDangerStarted;

    /// <summary>Strefa mrowiska znów jest bezpieczna - akcja DANGER dobiegła końca.</summary>
    public event Action OnDangerCleared;

    /// <summary>Konkretny wróg pojawił się w strefie - mrówki podejmują decyzję.</summary>
    public event Action<EnemyUnit> OnThreatAppeared;

    /// <summary>
    /// Konkretny wróg opuścił strefę mrowiska. Śmierć wroga nie wysyła tego eventu
    /// (nie podajemy martwego obiektu) - jego wygaśnięcie ogłasza <see cref="OnDangerCleared"/>.
    /// </summary>
    public event Action<EnemyUnit> OnThreatRemoved;

    /// <summary>Czy w strefie mrowiska trwa akcja DANGER.</summary>
    public bool IsDanger => threats.Count > 0;

    /// <summary>Aktualne zagrożenia w strefie (tylko do odczytu).</summary>
    public IReadOnlyList<EnemyUnit> ActiveThreats => threats;

    /// <summary>Środek strefy - używany też w gizmosach, gdy anthillCenter jest pusty.</summary>
    public Vector3 ZoneCenter => anthillCenter != null ? anthillCenter.position : transform.position;

    private void Update()
    {
        if (Time.time < nextPoll) return;
        nextPoll = Time.time + pollInterval;

        RefreshThreats();
    }

    /// <summary>Dopisuje wroga do obserwowanych (wołane przez EnemyUnit przy włączeniu).</summary>
    public void Register(EnemyUnit enemy)
    {
        if (enemy == null || registered.Contains(enemy)) return;

        registered.Add(enemy);

        // Wróg może pojawić się już w strefie - sprawdzamy od razu, bez czekania na odświeżenie.
        Evaluate(enemy);
    }

    /// <summary>Zdejmuje wroga z obserwowanych (wołane przez EnemyUnit przy wyłączeniu).</summary>
    public void Unregister(EnemyUnit enemy)
    {
        // Remove działa też dla zniszczonego obiektu (Unity Equals), więc nie blokujemy
        // się na "enemy == null" - inaczej martwe wpisy zostalyby w rejestrze na zawsze.
        registered.Remove(enemy);
        RemoveThreatIfPresent(enemy);
    }

    /// <summary>Najbliższe aktywne zagrożenie w strefie albo null, gdy strefa jest czysta.</summary>
    public EnemyUnit NearestThreat(Vector3 from)
    {
        EnemyUnit nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < threats.Count; i++)
        {
            EnemyUnit threat = threats[i];
            if (threat == null || threat.IsDead) continue;

            float sqrDistance = (threat.transform.position - from).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = threat;
        }

        return nearest;
    }

    /// <summary>Przelicza, kto aktualnie jest w strefie, i odpala eventy DANGER.</summary>
    private void RefreshThreats()
    {
        // Kopiujemy listę, bo eventy mogą rejestrować/wyrejestrowywać wrogów w trakcie pętli.
        pollBuffer.Clear();
        pollBuffer.AddRange(registered);

        for (int i = 0; i < pollBuffer.Count; i++)
            Evaluate(pollBuffer[i]);
    }

    private void Evaluate(EnemyUnit enemy)
    {
        // Wróg zginął albo obiekt został zniszczony - wyrzucamy go ze stanu strefy.
        if (enemy == null || enemy.IsDead)
        {
            registered.Remove(enemy);
            RemoveThreatIfPresent(enemy);
            return;
        }

        bool inside = IsInDangerZone(enemy);
        bool known = threats.Contains(enemy);

        if (inside && !known) AddThreat(enemy);
        else if (!inside && known) RemoveThreatIfPresent(enemy);
    }

    private bool IsInDangerZone(EnemyUnit enemy)
    {
        if (enemy == null || enemy.IsDead) return false;

        float sqrDistance = (enemy.transform.position - ZoneCenter).sqrMagnitude;
        return sqrDistance <= dangerRadius * dangerRadius;
    }

    private void AddThreat(EnemyUnit enemy)
    {
        bool dangerWasOff = threats.Count == 0;
        threats.Add(enemy);

        if (dangerWasOff)
        {
            Log("DANGER - wróg wszedł do strefy mrowiska.");
            OnDangerStarted?.Invoke();
        }

        OnThreatAppeared?.Invoke(enemy);
    }

    private void RemoveThreatIfPresent(EnemyUnit enemy)
    {
        if (!threats.Remove(enemy)) return;

        // Zmarłego/zniszczonego wroga nie podajemy dalej - abonent mógłby sięgnąć po jego
        // transform. Żywemu tylko zeszło ze strefy wysyłamy event wyjścia.
        if (enemy != null && !enemy.IsDead)
            OnThreatRemoved?.Invoke(enemy);

        if (threats.Count == 0)
        {
            Log("Strefa mrowiska bezpieczna - koniec akcji DANGER.");
            OnDangerCleared?.Invoke();
        }
    }

    private void Log(string message)
    {
        if (enableLogs) Debug.Log($"[DangerDirector] {message}", this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ZoneCenter, dangerRadius);
    }
}
