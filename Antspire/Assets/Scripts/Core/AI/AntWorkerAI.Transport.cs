using UnityEngine;

/// <summary>
/// Część <see cref="AntWorkerAI"/>: transport zasobów. Mrówka pobiera zlecenie
/// z <see cref="TransportBoard"/>, jedzie do źródła, zabiera ładunek, wiezie go do celu
/// i wraca po kolejne. Zlecenie przerywane (zgon, zniszczenie) wraca do kolejki.
/// </summary>
public partial class AntWorkerAI
{
    private TransportRequest activeTransport;
    private bool isCarried;

    private bool TryStartTransport()
    {
        if (transportBoard == null) return false;
        if (!transportBoard.TryTakeNext(out TransportRequest request)) return false;

        activeTransport = request;
        isCarried = false;
        ResumeTransport();
        return true;
    }

    private void ResumeTransport()
    {
        state = AntAIState.Moving;
        Vector3 destination = isCarried
            ? activeTransport.Target.transform.position
            : activeTransport.Source.transform.position;
        mover.MoveTo(destination);
    }

    private void HandleTransportArrival()
    {
        if (!isCarried)
        {
            PickUpTransport();
            return;
        }

        activeTransport.Deliver();
        transportBoard.Complete(activeTransport);
        activeTransport = null;
        isCarried = false;
        MirrorCarried();
        SearchRoutine();
    }

    private void PickUpTransport()
    {
        if (!activeTransport.TryPickUp())
        {
            // źródło nie ma już zasobu - zlecenie jest do wyrzucenia.
            transportBoard.Cancel(activeTransport);
            activeTransport = null;
            SearchRoutine();
            return;
        }

        isCarried = true;
        MirrorCarried();
        mover.MoveTo(activeTransport.Target.transform.position);
    }

    private void DropCarriedTransport()
    {
        if (activeTransport == null) return;

        // Zlecenie wraca do kolejki - inna mrówka dowiezie ładunek.
        if (transportBoard != null) transportBoard.Release(activeTransport);

        activeTransport = null;
        isCarried = false;
        MirrorCarried();
    }

    /// <summary>Lustruje ilość niesionych zasobów do komponentu Ant (UI/zapis gry).</summary>
    private void MirrorCarried()
    {
        if (ant == null) return;

        ant.carriedResources = isCarried && activeTransport != null ? activeTransport.Amount : 0;
    }
}
