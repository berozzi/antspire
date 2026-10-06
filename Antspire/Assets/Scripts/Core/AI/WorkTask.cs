using System;
using UnityEngine;

/// <summary>
/// Rodzaj pracy wykonywanej w miejscu pracy. Lista jest otwarta - dodaj nowe wartości,
/// gdy wprowadzisz kolejne rodzaje pracy mrówek.
/// </summary>
public enum WorkTaskKind
{
    Generic,
    Gathering,
    Processing,
    Hauling,
    Maintenance,
    Research,
    Guarding
}

/// <summary>
/// Pojedynczy task możliwy do wykonania w miejscu pracy (<see cref="Workplace"/>).
/// Czas trwania stroisz bezpośrednio w Inspektorze miejsca pracy.
/// </summary>
[Serializable]
public class WorkTaskDefinition
{
    [SerializeField] private string taskId = "task";
    [SerializeField] private WorkTaskKind kind = WorkTaskKind.Generic;
    [SerializeField] private float durationSeconds = 5f;

    public string TaskId => string.IsNullOrEmpty(taskId) ? "task" : taskId;
    public WorkTaskKind Kind => kind;

    /// <summary>Czas trwania taska w sekundach (zawsze dodatni, żeby pusty task nie zawiesił AI).</summary>
    public float DurationSeconds => Mathf.Max(0.05f, durationSeconds);

    public override string ToString() => TaskId;
}
