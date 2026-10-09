using System;
using UnityEngine;

/// <summary>
/// Jednostka wroga atakująca mrówki. Co <see cref="aggroCheckInterval"/> sekundy szuka
/// najbliższej żywej mrówki w zasięgu i, gdy ta jest w <see cref="attackRange"/>,
/// zadaje jej obrażenia. <see cref="Strength"/> (siła) jest porównywana z siłą mrówki -
/// większa siła sprawia, że mrówka instynktownie ucieka zamiast walczyć.
/// Przy włączeniu wróg rejestruje się w <see cref="DangerDirector"/>, który pilnuje
/// strefy mrowiska i odpala akcję DANGER dla mrówek defensywnych i eksplorujących.
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

    // Rejestracja w dyrektorze zagrożeń - źródło akcji DANGER dla mrówek.
    private DangerDirector dangerDirector;
    private float directorRetryTimer;

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
        TickDangerRegistration();

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

    private void OnEnable()
    {
        directorRetryTimer = 0f;
        EnsureDirectorRegistered();
    }

    private void OnDisable()
    {
        // Wyłączony wróg znika ze strefy - mrówki dostają koniec akcji DANGER.
        if (dangerDirector != null) dangerDirector.Unregister(this);
    }

    /// <summary>Dopisuje tego wroga do <see cref="DangerDirector"/> - stąd bierze się akcja DANGER.</summary>
    private void EnsureDirectorRegistered()
    {
        if (dangerDirector == null) dangerDirector = FindAnyObjectByType<DangerDirector>();
        if (dangerDirector == null) return;

        dangerDirector.Register(this);
    }

    /// <summary>Ponawia szukanie dyrektora, gdy ten nie istniał jeszcze przy włączeniu wroga.</summary>
    private void TickDangerRegistration()
    {
        if (dangerDirector != null) return;

        directorRetryTimer -= Time.deltaTime;
        if (directorRetryTimer > 0f) return;

        directorRetryTimer = 1f;
        EnsureDirectorRegistered();
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
