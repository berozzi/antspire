using UnityEngine;

/// <summary>
/// Reakcja mrówki na atak - osobny komponent, niezależny od pętli rutyny.
/// Nasłuchuje <see cref="AntHealth.OnDamaged"/> i podejmuje decyzję: napastnik o większej
/// sile (<see cref="strength"/>) → instynktowna ucieczka, w przeciwnym razie kontraatak.
/// W trakcie walki mrówka jest zamrożona w <see cref="AntWorkerAI"/> (stany Battling/Fleeing),
/// a po zakończeniu komponent woła <see cref="AntWorkerAI.ResumeAfterCombat"/>,
/// żeby wrócić do przerwanego taska. Zgon mrówki kasuje cel.
/// </summary>
[RequireComponent(typeof(AntHealth), typeof(AntWorkerAI), typeof(AntMover))]
public class AntThreatResponse : MonoBehaviour
{
    [Header("Walka")]
    [SerializeField] private float strength = 5f;
    [SerializeField] private float meleeDamage = 2f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chasePathInterval = 0.5f;

    [Header("Ucieczka")]
    [SerializeField] private float fleeDuration = 5f;
    [SerializeField] private float fleeDistance = 12f;
    [SerializeField] private float fleeSampleRadius = 6f;

    private AntHealth health;
    private AntWorkerAI worker;
    private AntMover mover;

    private EnemyUnit target;
    private float attackTimer;
    private float nextChasePath;
    private float fleeEndsAt;

    private void Awake()
    {
        health = GetComponent<AntHealth>();
        worker = GetComponent<AntWorkerAI>();
        mover = GetComponent<AntMover>();
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    private void Update()
    {
        if (target == null) return;

        if (worker.State == AntAIState.Fleeing) TickFleeing();
        else TickBattling();
    }

    private void HandleDamaged(EnemyUnit attacker)
    {
        if (attacker == null || attacker.IsDead || target != null) return;

        // Instynkt: większa siła napastnika = ucieczka, w przeciwnym razie kontraatak.
        if (attacker.Strength > strength) StartFleeing(attacker);
        else StartBattling(attacker);
    }

    private void HandleDied() => target = null;

    private void StartBattling(EnemyUnit attacker)
    {
        target = attacker;
        attackTimer = 0f;
        nextChasePath = 0f;
        worker.SetState(AntAIState.Battling);
    }

    private void StartFleeing(EnemyUnit attacker)
    {
        target = attacker;
        fleeEndsAt = Time.time + fleeDuration;
        worker.SetState(AntAIState.Fleeing);
        mover.MoveTo(GetFleePoint(attacker));
    }

    private void TickBattling()
    {
        if (target == null || target.IsDead)
        {
            EndCombat();
            return;
        }

        if (mover.IsInReach(target.transform.position, attackRange))
        {
            ExchangeBlows();
            return;
        }

        ChaseTarget();
    }

    private void ExchangeBlows()
    {
        if (!mover.IsStopped) mover.Stop();

        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        if (attackTimer > 0f) return;

        attackTimer = attackInterval;
        target.TakeDamage(meleeDamage);
    }

    private void ChaseTarget()
    {
        if (Time.time < nextChasePath) return;

        nextChasePath = Time.time + chasePathInterval;
        mover.MoveTo(target.transform.position);
    }

    private void TickFleeing()
    {
        bool safe = target == null || target.IsDead || Time.time >= fleeEndsAt || mover.HasArrived;
        if (safe || mover.PathFailed) EndCombat();
    }

    private void EndCombat()
    {
        target = null;
        worker.ResumeAfterCombat();
    }

    /// <summary>Punkt ucieczki w linii prostej od napastnika, próbkowany na NavMeshie.</summary>
    private Vector3 GetFleePoint(EnemyUnit attacker)
    {
        Vector3 away = transform.position - attacker.transform.position;
        if (away.sqrMagnitude < 0.0001f) away = Vector3.forward;
        else away.Normalize();

        Vector3 candidate = transform.position + away * fleeDistance;
        return mover.TrySamplePoint(candidate, fleeSampleRadius, out Vector3 point) ? point : candidate;
    }
}
