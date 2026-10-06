using UnityEngine;

/// <summary>
/// Zlecenie transportu zasobu: przenieś <see cref="Amount"/> × <see cref="Resource"/>
/// ze <see cref="Source"/> do <see cref="Target"/>. Zleceniem zajmuje się jedna mrówka
/// od pobrania (<see cref="TransportBoard.TryTakeNext"/>) do ukończenia lub porzucenia.
/// </summary>
public sealed class TransportRequest
{
    public TransportRequest(ResourceDef resource, int amount, ResourceStorage source, ResourceStorage target)
    {
        Resource = resource;
        Amount = amount;
        Source = source;
        Target = target;
    }

    public ResourceDef Resource { get; }
    public int Amount { get; }
    public ResourceStorage Source { get; }
    public ResourceStorage Target { get; }

    /// <summary>Czy zlecenie nadal ma sens (zasób, ilość, magazyny niezniszczone i różne od siebie).</summary>
    public bool IsValid =>
        Resource != null &&
        Amount > 0 &&
        Source != null &&
        Target != null &&
        Source != Target;

    /// <summary>Czy zlecenie jest właśnie obsługiwane przez jakąś mrówkę.</summary>
    public bool IsClaimed { get; internal set; }

    /// <summary>Zabiera cały ładunek ze źródła. Zwraca false, gdy źródło nie ma już zasobu.</summary>
    public bool TryPickUp()
    {
        return IsValid && Source.CanWithdraw(Resource, Amount) && Source.TryWithdraw(Resource, Amount);
    }

    /// <summary>Oddaje ładunek do magazynu docelowego.</summary>
    public void Deliver()
    {
        if (Resource != null && Amount > 0 && Target != null)
            Target.AddResource(Resource, Amount);
    }

    public override string ToString()
    {
        return $"{Amount}x {Resource} : {Source} -> {Target}";
    }
}
