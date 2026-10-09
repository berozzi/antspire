/// <summary>
/// Rola mrówki ustawiana w Inspektorze albo przez panel (WorkAssignmentPanelUI).
/// Obecnie rola jest informacyjna - AI każdej mrówki i tak tylko transportuje towar
/// między dwoma punktami (originPoint → destinationPoint). Jedyny czynny wyjątek:
/// <see cref="AntThreatResponse"/> sprawdza <see cref="AntRole.Defense"/>, żeby
/// mrówka defensywna nigdy nie uciekała z walki.
/// Kolejność wartości odpowiada indeksom listy w panelu - nowe role dopisujemy
/// na końcu, żeby nie przesunąć już zapisanych indeksów.
/// </summary>
public enum AntRole
{
    /// <summary>Praca - historyczna; mrówki i tak nie pracują, tylko transportują.</summary>
    Work,

    /// <summary>Defensywa: nigdy nie ucieka z walki (jedyny aktywny efekt roli).</summary>
    Defense,

    /// <summary>Eksploracja - zarezerwowane na przyszłość.</summary>
    Explore,

    /// <summary>Transport - domyślna rola: zawsze transport między dwoma punktami.</summary>
    Transport
}