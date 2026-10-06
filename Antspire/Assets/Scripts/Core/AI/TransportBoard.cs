using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tablica zleceń transportu. Nadawcy (budynki, magazyny, gracze) dodają zlecenia przez
/// <see cref="Post"/>, a mrówki-workery pobierają je przez <see cref="TryTakeNext"/>.
/// Pobranie zaznacza zlecenie jako zajęte - do czasu <see cref="Complete"/> (udany transport),
/// <see cref="Release"/> (mrówka przerwała, np. zginęła - zlecenie wraca do kolejki)
/// albo <see cref="Cancel"/> (zlecenie do unieważnienia).
/// </summary>
public class TransportBoard : MonoBehaviour
{
    private readonly List<TransportRequest> pending = new List<TransportRequest>();

    /// <summary>Liczba zleceń w kolejce (łącznie z tymi w trakcie transportu).</summary>
    public int PendingCount => pending.Count;

    /// <summary>Dodaje zlecenie transportu na koniec kolejki.</summary>
    public void Post(TransportRequest request)
    {
        if (request == null || !request.IsValid) return;
        pending.Add(request);
    }

    /// <summary>Bierze najstarsze wolne zlecenie (FIFO) i przypisuje je mrówce.</summary>
    public bool TryTakeNext(out TransportRequest request)
    {
        for (int i = 0; i < pending.Count; i++)
        {
            TransportRequest candidate = pending[i];

            if (!candidate.IsValid)
            {
                pending.RemoveAt(i);
                i--;
                continue;
            }

            if (candidate.IsClaimed) continue;

            candidate.IsClaimed = true;
            request = candidate;
            return true;
        }

        request = null;
        return false;
    }

    /// <summary>Kończy zlecenie po udanym dostarczeniu ładunku.</summary>
    public void Complete(TransportRequest request)
    {
        if (request != null) pending.Remove(request);
    }

    /// <summary>Zwraca zlecenie do kolejki - mrówka przerwała transport (np. zginęła podczas drogi).</summary>
    public void Release(TransportRequest request)
    {
        if (request != null) request.IsClaimed = false;
    }

    /// <summary>Unieważnia zlecenie, którego nie da się wykonać (np. źródło zostało opróżnione).</summary>
    public void Cancel(TransportRequest request)
    {
        if (request != null) pending.Remove(request);
    }
}
