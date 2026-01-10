using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CombatUnit : MonoBehaviour
{
    [Header("Combat Stats")]
    public int maxHealth = 100;
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Team")]
    public bool isPlayerUnit = true;

    private int currentHealth;
    private CombatUnit currentTarget;
    private float nextAttackTime;
    private NavMeshAgent agent;
    private static bool gameOver = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        transform.Rotate(0f, 0f, 180f);
    }

    void Start()
    {
        currentHealth = maxHealth;
        agent.stoppingDistance = Mathf.Min(attackRange, 3.0f);
        agent.radius = 0.5f;
        agent.height = 2.0f;
        agent.baseOffset = 0f;
    }

    void Update()
    {
        if (gameOver) return;

        FindTarget();
        if (currentTarget == null)
        {
            agent.isStopped = true;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        agent.isStopped = distance <= attackRange;

        if (distance > attackRange)
            agent.SetDestination(currentTarget.transform.position);
        else if (Time.time >= nextAttackTime)
            Attack();
    }

    void FindTarget()
    {
        if (currentTarget != null && currentTarget.currentHealth > 0) return;

        CombatUnit[] allUnits = GameObject.FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        CombatUnit closestEnemy = null;

        foreach (CombatUnit unit in allUnits)
        {
            if (unit == null || unit == this || unit.isPlayerUnit == isPlayerUnit || unit.currentHealth <= 0)
                continue;

            float distance = Vector3.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = unit;
            }
        }

        currentTarget = closestEnemy;
    }

    void Attack()
    {
        if (currentTarget == null) return;
        currentTarget.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
        Debug.Log($"{name} attacked {currentTarget.name} for {damage} damage! | {name} HP: {currentHealth}/{maxHealth} | {currentTarget.name} HP: {currentTarget.currentHealth}/{currentTarget.maxHealth}");
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        yield return new WaitForEndOfFrame();
        CheckGameOver();
        Destroy(gameObject);
    }

    void CheckGameOver()
    {
        CombatUnit[] allUnits = GameObject.FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        bool playerHasUnits = false;
        bool enemyHasUnits = false;

        foreach (CombatUnit unit in allUnits)
        {
            if (unit == null || unit.currentHealth <= 0) continue;
            if (unit.isPlayerUnit) playerHasUnits = true;
            else enemyHasUnits = true;
        }

        if (!playerHasUnits && !enemyHasUnits)
        {
            Debug.Log("=== DRAW - Both sides eliminated! ===");
            gameOver = true;
            StopAllUnits();
        }
        else if (!playerHasUnits)
        {
            Debug.Log("=== GAME OVER - All player units dead! ===");
            gameOver = true;
            StopAllUnits();
        }
        else if (!enemyHasUnits)
        {
            Debug.Log("=== YOU WON - All enemies dead! ===");
            gameOver = true;
            StopAllUnits();
        }
    }

    void StopAllUnits()
    {
        foreach (CombatUnit unit in GameObject.FindObjectsByType<CombatUnit>(FindObjectsSortMode.None))
        {
            if (unit == null) continue;
            unit.enabled = false;
            if (unit.agent != null && unit.agent.isOnNavMesh && unit.agent.enabled)
                unit.agent.isStopped = true;
        }
    }
}