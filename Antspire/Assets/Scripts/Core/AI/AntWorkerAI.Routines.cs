using UnityEngine;

/// <summary>
/// Część <see cref="AntWorkerAI"/>: szukanie kolejnej rutyny oraz jej stany
/// (Idle, Moving, Working, Exploring). Rutyna zwalnia się przed kolejną -
/// po każdym ukończonym tasku mrówka od razu szuka następnej.
/// </summary>
public partial class AntWorkerAI
{
    /// <summary>Szukanie kolejnej rutyny - starcie, po każdym ukończonym tasku i po walce bez czegoś do wznowienia.</summary>
    private void SearchRoutine()
    {
        ReleaseHeldRoutine();

        if (TryStartTransport()) return;
        if (TryStartWork()) return;
        if (TryStartExploration()) return;

        state = AntAIState.Idle;
        nextRoutineSearch = Time.time + routineRetryInterval;
    }

    private bool TryStartWork()
    {
        if (workplacePools == null) return false;
        if (!workplacePools.TryClaimNext(out Workplace workplace)) return false;

        claimedWorkplace = workplace;
        currentTask = workplace.PickRandomTask();
        ResumeWork();
        return true;
    }

    private bool TryStartExploration()
    {
        Vector3 origin = explorationCenter != null ? explorationCenter.position : transform.position;
        Vector3 candidate = origin + Random.insideUnitSphere * explorationRadius;

        if (!mover.TrySamplePoint(candidate, explorationSampleRadius, out Vector3 point)) return false;

        dwellTimer = -1f;
        state = AntAIState.Exploring;
        mover.MoveTo(point);
        return true;
    }

    /// <summary>Zwalnia wszystko, co mrówka trzymała - przed nową rutyną, przy zgonie i zniszczeniu.</summary>
    private void ReleaseHeldRoutine()
    {
        ReleaseWorkplace();
        DropCarriedTransport();
    }

    private void ReleaseWorkplace()
    {
        if (claimedWorkplace == null) return;

        claimedWorkplace.Release();
        claimedWorkplace = null;
        currentTask = null;
    }

    /// <summary>Nic do roboty albo cel nieosiągalny - czekamy i ponawiamy szukanie rutyny.</summary>
    private void CancelRoutine()
    {
        ReleaseHeldRoutine();
        mover.Stop();
        state = AntAIState.Idle;
        nextRoutineSearch = Time.time + routineRetryInterval;
    }

    private void TickIdle()
    {
        if (Time.time < nextRoutineSearch) return;
        SearchRoutine();
    }

    private void TickMoving()
    {
        if (mover.HasArrived)
        {
            HandleRoutineArrival();
            return;
        }

        if (mover.PathFailed) CancelRoutine();
    }

    private void HandleRoutineArrival()
    {
        if (activeTransport != null)
        {
            HandleTransportArrival();
            return;
        }

        if (claimedWorkplace != null)
        {
            BeginWork();
            return;
        }

        SearchRoutine(); // brak celu - bezpieczne wyjście do szukania rutyny od nowa
    }

    private void BeginWork()
    {
        taskTimer = currentTask != null ? currentTask.DurationSeconds : defaultTaskDuration;
        state = AntAIState.Working;
        mover.Stop();
    }

    private void TickWorking()
    {
        taskTimer -= Time.deltaTime;
        if (taskTimer > 0f) return;

        // Efekt ukończonego taska (np. cykl produkcji budynku) - miejsce na rozszerzenie tutaj.
        SearchRoutine(); // task skończony -> zwalniamy etat i od razu szukamy następnej rutyny
    }

    private void TickExploring()
    {
        if (dwellTimer < 0f)
        {
            if (mover.PathFailed)
            {
                CancelRoutine();
                return;
            }
            if (!mover.HasArrived) return;

            mover.Stop();
            dwellTimer = explorationDwell;
        }

        dwellTimer -= Time.deltaTime;
        if (dwellTimer > 0f) return;

        dwellTimer = -1f;
        SearchRoutine();
    }

    private void ResumeWork()
    {
        state = AntAIState.Moving;
        mover.MoveTo(claimedWorkplace.transform.position);
    }
}
