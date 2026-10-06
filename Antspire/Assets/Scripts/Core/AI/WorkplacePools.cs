using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trzy pule miejsc pracy posegregowane wg obsadzenia (occupancy):
/// <list type="bullet">
/// <item>ASAP - obsadzenie poniżej progu (domyślnie &lt; 50%) - miejsce pilnie potrzebuje mrówek,</item>
/// <item>second order - obsadzenie między progami (domyślnie 50-75%),</item>
/// <item>third order - obsadzenie powyżej progu (domyślnie &gt; 75%) - najmniej pilne.</item>
/// </list>
/// Pule nie przeszukują sceny cyklicznie: miejsca pracy rejestrują się same
/// (<see cref="Workplace.OnEnable"/> → <see cref="Register"/>), a każde zajęcie albo zwolnienie
/// etatu od razu woła <see cref="Reclassify"/>. Dzięki temu pule są aktualne w chwili zmiany,
/// a nie co X sekund. Jedyny skan sceny to <see cref="RegisterAllFromScene"/> przy starcie -
/// łapie miejsca pracy, które włączyły się, zanim pule powstały.
/// Kolejność wydawania: ASAP → second order → third order, a wewnątrz puli miejsce o najniższej obsadzie.
/// </summary>
public class WorkplacePools : MonoBehaviour
{
    [Header("Progi obsadzenia (%)")]
    [SerializeField] private float asapThreshold = 50f;
    [SerializeField] private float secondOrderThreshold = 75f;

    private readonly List<Workplace> asapPool = new List<Workplace>();
    private readonly List<Workplace> secondOrderPool = new List<Workplace>();
    private readonly List<Workplace> thirdOrderPool = new List<Workplace>();

    /// <summary>Liczba miejsc pracy w puli ASAP (do UI/debugu).</summary>
    public int AsapCount => asapPool.Count;
    /// <summary>Liczba miejsc pracy w puli second order (do UI/debugu).</summary>
    public int SecondOrderCount => secondOrderPool.Count;
    /// <summary>Liczba miejsc pracy w puli third order (do UI/debugu).</summary>
    public int ThirdOrderCount => thirdOrderPool.Count;

    private void Start()
    {
        RegisterAllFromScene();
    }

    /// <summary>
    /// Buduje pule od zera na podstawie miejsc pracy obecnych w scenie.
    /// Wołane przy starcie (i ręcznie z konsoli debugu, gdy trzeba wymusić przeliczenie).
    /// </summary>
    public void RegisterAllFromScene()
    {
        asapPool.Clear();
        secondOrderPool.Clear();
        thirdOrderPool.Clear();

        foreach (Workplace workplace in FindObjectsByType<Workplace>())
            Register(workplace);
    }

    /// <summary>Dodaje miejsce pracy do puli zgodnej z jego obsadzeniem. Idempotentne.</summary>
    public void Register(Workplace workplace)
    {
        if (workplace == null || IsRegistered(workplace)) return;

        workplace.AttachPool(this);
        PoolFor(workplace).Add(workplace);
    }

    /// <summary>Zdejmuje miejsce pracy z puli (wołane przy wyłączeniu/zniszczeniu miejsca pracy).</summary>
    public void Unregister(Workplace workplace)
    {
        if (workplace == null) return;

        asapPool.Remove(workplace);
        secondOrderPool.Remove(workplace);
        thirdOrderPool.Remove(workplace);
    }

    /// <summary>Przenosi miejsce pracy do puli pasującej do jego aktualnego obsadzenia.</summary>
    public void Reclassify(Workplace workplace)
    {
        if (workplace == null) return;

        Unregister(workplace);
        PoolFor(workplace).Add(workplace);
    }

    /// <summary>
    /// Próbuje przydzielić następne wolne miejsce pracy. Kolejka: ASAP, potem second order,
    /// na końcu third order. Zajęcie etatu samo przeniesie miejsce do właściwej puli.
    /// </summary>
    public bool TryClaimNext(out Workplace workplace)
    {
        workplace = FindBestCandidate(asapPool)
                 ?? FindBestCandidate(secondOrderPool)
                 ?? FindBestCandidate(thirdOrderPool);

        if (workplace == null) return false;

        // Pojedyncze zajęcie etatu - wykonywane po zakończeniu przeszukiwania pul,
        // żeby zmiana obsadzenia nie przestawiała listy, po której właśnie idziemy.
        if (!workplace.TryOccupy())
        {
            workplace = null;
            return false;
        }

        return true;
    }

    /// <summary>Zwraca miejsce z puli o najniższej obsadzie i choć jednym wolnym etatem.</summary>
    private static Workplace FindBestCandidate(List<Workplace> pool)
    {
        Workplace best = null;

        for (int i = 0; i < pool.Count; i++)
        {
            Workplace candidate = pool[i];
            if (candidate == null || candidate.AvailableSpots <= 0) continue;
            if (best != null && candidate.OccupationPercent >= best.OccupationPercent) continue;

            best = candidate;
        }

        return best;
    }

    private List<Workplace> PoolFor(Workplace workplace)
    {
        float occupation = workplace.OccupationPercent;

        if (occupation < asapThreshold) return asapPool;
        if (occupation <= secondOrderThreshold) return secondOrderPool;
        return thirdOrderPool;
    }

    private bool IsRegistered(Workplace workplace)
    {
        return asapPool.Contains(workplace) ||
               secondOrderPool.Contains(workplace) ||
               thirdOrderPool.Contains(workplace);
    }
}
