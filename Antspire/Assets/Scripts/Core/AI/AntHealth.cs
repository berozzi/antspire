using System;
using UnityEngine;

/// <summary>
/// Zdrowie mrówki - jedyne miejsce przyjmowania obrażeń. Po każdym trafieniu odpala
/// <see cref="OnDamaged"/> (z atakującym jako argumentem), a po zejściu do zera <see cref="OnDied"/>.
/// </summary>
public class AntHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public bool IsDead => Current <= 0f;

    /// <summary>Odpalany, gdy mrówka otrzyma obrażenia - argument to atakujący.</summary>
    public event Action<EnemyUnit> OnDamaged;

    /// <summary>Odpalany, gdy mrówka zginie.</summary>
    public event Action OnDied;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(float amount, EnemyUnit attacker)
    {
        if (IsDead || amount <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        OnDamaged?.Invoke(attacker);

        if (IsDead) OnDied?.Invoke();
    }
}
