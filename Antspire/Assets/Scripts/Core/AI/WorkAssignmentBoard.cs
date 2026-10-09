using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pojedynczy przydział pracy: mrówka ma pracować na wskazanym przez gracza miejscu pracy.
/// Wpisy można układać bezpośrednio w Inspektorze tablicy albo dodawać w runtime (UI).
/// </summary>
[Serializable]
public class WorkAssignment
{
    [SerializeField] private AntWorkerAI ant;
    [SerializeField] private Workplace workplace;

    /// <summary>Mrówka, która ma pracować w tym miejscu.</summary>
    public AntWorkerAI Ant => ant;

    /// <summary>Miejsce pracy przypisane graczem.</summary>
    public Workplace Workplace => workplace;

    public WorkAssignment() { }

    public WorkAssignment(AntWorkerAI ant, Workplace workplace)
    {
        this.ant = ant;
        this.workplace = workplace;
    }
}

/// <summary>
/// Tablica przydziałów "mrówka -> miejsce pracy" tworzona przez gracza.
/// To ona decyduje, gdzie konkretna mrówka pracuje: <see cref="AntWorkerAI"/>
/// o roli <see cref="AntRole.Work"/> pyta wyłącznie o przydział tutaj - nie ma
/// żadnego automatycznego wybierania pracy.
/// Nie jest singletonem - referencję podkładasz w Inspektorze albo panel szuka jej w scenie.
/// </summary>
public class WorkAssignmentBoard : MonoBehaviour
{
    [SerializeField] private List<WorkAssignment> assignments = new List<WorkAssignment>();

    [Header("Konfiguracja")]
    [SerializeField] private bool enableLogs = true;

    /// <summary>Wszystkie zapisane przydziały (tylko do odczytu - do UI i debugu).</summary>
    public IReadOnlyList<WorkAssignment> Assignments => assignments;

    /// <summary>
    /// Szuka miejsca pracy przypisanego graczem do podanej mrówki.
    /// Przy okazji czyści wpisy z zniszczonymi mrówkami albo miejscami pracy.
    /// </summary>
    public bool TryGetWorkplace(AntWorkerAI ant, out Workplace workplace)
    {
        workplace = null;
        if (ant == null) return false;

        for (int i = assignments.Count - 1; i >= 0; i--)
        {
            WorkAssignment entry = assignments[i];

            // Wpisy po zniszczeniu obiektu są bezużyteczne - usuwamy je przy okazji szukania.
            // Uwaga: ta gałąź tylko KASUJE wpisy, które już są martwe (pusta referencja) -
            // nigdy nie zeruje poprawnie przypisanego workplace.
            if (entry == null || entry.Ant == null || entry.Workplace == null)
            {
                assignments.RemoveAt(i);
                Debug.LogWarning(
                    "[WorkAssignmentBoard] Usunięto martwy wpis przydziału" +
                    (entry != null && entry.Ant == null ? " (brak mrówki)" : "") +
                    (entry != null && entry.Workplace == null ? " (brak miejsca pracy)" : "") +
                    ". Przydział nadawany w Play Mode wraca do stanu sprzed gry po wyjściu.",
                    this);
                continue;
            }

            if (entry.Ant != ant) continue;

            workplace = entry.Workplace;
            return true;
        }

        return false;
    }

    /// <summary>Przypisuje mrówkę do miejsca pracy (zastępuje jej poprzedni przydział).</summary>
    public void Assign(AntWorkerAI ant, Workplace workplace)
    {
        if (ant == null || workplace == null)
        {
            Debug.LogWarning("[WorkAssignmentBoard] Przydział odrzucony: brak mrówki albo miejsca pracy.", this);
            return;
        }

        for (int i = 0; i < assignments.Count; i++)
        {
            WorkAssignment entry = assignments[i];
            if (entry == null || entry.Ant != ant) continue;

            assignments[i] = new WorkAssignment(ant, workplace);
            Log($"Przypisano {ant.name} -> {workplace.name} (podmiana).");
            return;
        }

        assignments.Add(new WorkAssignment(ant, workplace));
        Log($"Przypisano {ant.name} -> {workplace.name}.");
    }

    /// <summary>Przypisuje kilka mrówek do jednego miejsca pracy naraz.</summary>
    public void Assign(IEnumerable<AntWorkerAI> ants, Workplace workplace)
    {
        if (ants == null || workplace == null) return;

        foreach (AntWorkerAI ant in ants)
            Assign(ant, workplace);
    }

    /// <summary>Zdejmuje przydział mrówki. Zwraca true, gdy jakiś wpis istniał.</summary>
    public bool Unassign(AntWorkerAI ant)
    {
        if (ant == null) return false;

        for (int i = assignments.Count - 1; i >= 0; i--)
        {
            WorkAssignment entry = assignments[i];

            // Martwe wpisy (zniszczony obiekt / puste pola) czyścimy przy okazji,
            // ale ich NIE mylimy ze zdjęciem przydziału konkretnej mrówki.
            if (entry == null || entry.Ant == null || entry.Workplace == null)
            {
                assignments.RemoveAt(i);
                continue;
            }

            if (entry.Ant != ant) continue;

            assignments.RemoveAt(i);
            Log($"Zdjęto przydział {ant.name}.");
            return true;
        }

        return false;
    }

    /// <summary>Wypisuje wszystkie mrówki przypisane do danego miejsca pracy (do UI/debugu).</summary>
    public void GetAssignedAnts(Workplace workplace, List<AntWorkerAI> results)
    {
        if (results == null || workplace == null) return;

        results.Clear();
        for (int i = 0; i < assignments.Count; i++)
        {
            WorkAssignment entry = assignments[i];
            if (entry != null && entry.Workplace == workplace && entry.Ant != null)
                results.Add(entry.Ant);
        }
    }

    private void Log(string message)
    {
        if (enableLogs) Debug.Log($"[WorkAssignmentBoard] {message}", this);
    }
}
