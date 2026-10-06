using System;
using UnityEngine;

/// <summary>
/// Jednostka wroga atakująca mrówki. Co <see cref="aggroCheckInterval"/> sekundy szuka
/// najbliższej żywej mrówki w zasięgu i, gdy ta jest w <see cref="attackRange"/>,
/// zadaje jej obrażenia. <see cref="Strength"/> (siła) jest porównywana z siłą mrówki -
/// większa siła sprawia, że mrówka instynktownie ucieka zamiast walczyć.
/// Pogonią/ruchem wroga zajmuje się osobny system - tutaj jest celowanie i bicie.
/// </summary>
public class EnemyUnit : MonoBehaviour
{
    [Header("Parametry walki")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float strength = 3f;
    [SerializeField] private float damage = 2f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Wykrywanie mrówek")]
    [SerializeField] private float aggroRadius = 3f;
    [SerializeField] private float aggroCheckInterval = 0.5f;
    [SerializeField] private LayerMask antMask = ~0;

    private readonly Collider[] detectedColliders = new Collider[16];

    private float attackTimer;
    private float aggroTimer;

    /// <summary>Siła tej jednostki - jeśli przewyższa siłę mrówki, ta ucieka instynktownie.</summary>
    public float Strength => strength;

    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (IsDead) return;

        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        aggroTimer -= Time.deltaTime;
        if (aggroTimer > 0f) return;
        aggroTimer = aggroCheckInterval;

        AntHealth target = FindNearestAnt();
        if (target == null || attackTimer > 0f) return;
        if (!IsInAttackRange(target)) return;

        attackTimer = attackInterval;
        target.TakeDamage(damage, this);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (IsDead) OnDied?.Invoke();
    }

    private AntHealth FindNearestAnt()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, aggroRadius, detectedColliders, antMask);

        AntHealth nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!detectedColliders[i].TryGetComponent(out AntHealth ant) || ant.IsDead) continue;

            float sqrDistance = (ant.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = ant;
        }

        return nearest;
    }

    private bool IsInAttackRange(AntHealth target)
    {
        float sqrDistance = (target.transform.position - transform.position).sqrMagnitude;
        return sqrDistance <= attackRange * attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
