using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel przydziału pracy. Gracz zaznacza mrówki (pojedynczą albo kilka) oraz miejsce
/// pracy, a przycisk zapisuje przydział w <see cref="WorkAssignmentBoard"/> - stamtąd
/// pobiera go <see cref="AntWorkerAI"/> przy szukaniu rutyny.
/// Ten sam panel pozwala zmienię rolę zaznaczonych mrówek (praca/defensywa/eksploracja)
/// przez <see cref="SetSelectedRole"/> podpięte pod Dropdown albo UnityEvent.
/// Wszystkie pola UI są opcjonalne - bez nich panel działa i loguje do konsoli.
/// </summary>
public class WorkAssignmentPanelUI : MonoBehaviour
{
    [Header("Zależności (puste = szukane w scenie)")]
    [SerializeField] private WorkAssignmentBoard assignmentBoard;

    [Header("Wybór gracza")]
    [SerializeField] private Workplace workplace;
    [SerializeField] private List<AntWorkerAI> selectedAnts = new List<AntWorkerAI>();

    [Header("UI (opcjonalne)")]
    [SerializeField] private GameObject panelRoot; // korzeń panelu przełączany przyciskiem HUD
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button unassignButton;

    /// <summary>Mrówki zaznaczone w panelu (tylko do odczytu - do zewnętrznego zaznaczania).</summary>
    public IReadOnlyList<AntWorkerAI> SelectedAnts => selectedAnts;

    private void Awake()
    {
        if (assignmentBoard == null) assignmentBoard = FindAnyObjectByType<WorkAssignmentBoard>();

        if (assignButton != null) assignButton.onClick.AddListener(AssignSelected);
        if (unassignButton != null) unassignButton.onClick.AddListener(UnassignSelected);
    }

    private void Start() => RefreshStatus();

    /// <summary>Zaznacza mrówki do kolejnego przydziału (np. z zaznaczenia na mapie).</summary>
    public void SetSelectedAnts(IEnumerable<AntWorkerAI> ants)
    {
        selectedAnts.Clear();
        if (ants == null) return;

        foreach (AntWorkerAI ant in ants)
        {
            if (ant != null) selectedAnts.Add(ant);
        }

        RefreshStatus();
    }

    /// <summary>Ustawia miejsce pracy wskazane graczem.</summary>
    public void SetWorkplace(Workplace target)
    {
        workplace = target;
        RefreshStatus();
    }

    /// <summary>Włącza/wyłącza panel przydziałów - podłącz pod przycisk HUD.</summary>
    public void TogglePanel()
    {
        GameObject root = panelRoot != null ? panelRoot : gameObject;
        root.SetActive(!root.activeSelf);

        if (root.activeSelf) RefreshStatus();
    }

    /// <summary>Przypisuje zaznaczane mrówki do wybranego miejsca pracy - podłącz pod przycisk.</summary>
    public void AssignSelected()
    {
        if (!CanAssign(out string problem))
        {
            SetStatus(problem);
            return;
        }

        assignmentBoard.Assign(selectedAnts, workplace);
        SetStatus($"Przypisano {selectedAnts.Count} mrówek do: {workplace.name}.");
    }

    /// <summary>Zdejmuje przydział zaznaczanych mrówek - podłącz pod przycisk.</summary>
    public void UnassignSelected()
    {
        if (assignmentBoard == null)
        {
            SetStatus("Brak WorkAssignmentBoard w scenie - podłącz zależność w panelu.");
            return;
        }

        int removed = 0;
        foreach (AntWorkerAI ant in selectedAnts)
        {
            if (ant != null && assignmentBoard.Unassign(ant)) removed++;
        }

        SetStatus(removed > 0
            ? $"Zdjęto przydział z {removed} mrówek."
            : "Żadna z zaznaczonych mrówek nie miała przydziału.");
    }

    /// <summary>
    /// Ustawia rolę wszystkich zaznaczonych mrówek - podłącz pod Dropdown
    /// (indeksy odpowiadają wartościom <see cref="AntRole"/>).
    /// </summary>
    public void SetSelectedRole(int roleIndex)
    {
        if (!Enum.IsDefined(typeof(AntRole), roleIndex))
        {
            SetStatus($"Nieznany indeks roli: {roleIndex}.");
            return;
        }

        AntRole newRole = (AntRole)roleIndex;
        int changed = 0;

        foreach (AntWorkerAI ant in selectedAnts)
        {
            if (ant == null) continue;
            ant.SetRole(newRole);
            changed++;
        }

        SetStatus(changed > 0
            ? $"Ustawiono rolę {newRole} dla {changed} mrówek."
            : "Zaznacz przynajmniej jedną mrówkę.");
    }

    private bool CanAssign(out string problem)
    {
        if (assignmentBoard == null)
        {
            problem = "Brak WorkAssignmentBoard w scenie - podłącz zależność w panelu.";
            return false;
        }

        if (workplace == null)
        {
            problem = "Wskaż miejsce pracy.";
            return false;
        }

        if (selectedAnts.Count == 0)
        {
            problem = "Zaznacz przynajmniej jedną mrówkę.";
            return false;
        }

        problem = string.Empty;
        return true;
    }

    private void RefreshStatus()
    {
        string ants = selectedAnts.Count > 0 ? $"{selectedAnts.Count} mrówek" : "brak mrówek";
        string place = workplace != null ? workplace.name : "brak miejsca pracy";
        SetStatus($"Zaznaczenie: {ants} → {place}.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        Debug.Log($"[WorkAssignmentPanelUI] {message}", this);
    }
}
