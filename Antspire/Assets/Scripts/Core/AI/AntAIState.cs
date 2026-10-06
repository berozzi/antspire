/// <summary>
/// Stany, w których może znajdować się mrówka-worker sterowana przez <see cref="AntWorkerAI"/>.
/// </summary>
public enum AntAIState
{
    /// <summary>Brak rutyny - mrówka czeka na kolejną próbę jej znalezienia.</summary>
    Idle,

    /// <summary>Idzie do celu aktualnej rutyny (miejsce pracy albo źródło/cel transportu).</summary>
    Moving,

    /// <summary>Wykonuje task w przydzielonym miejscu pracy.</summary>
    Working,

    /// <summary>Wędruje po terytorium albo odpoczywa w dotartym punkcie eksploracji.</summary>
    Exploring,

    /// <summary>Walczy z przeciwnikiem, który ją zaatakował.</summary>
    Battling,

    /// <summary>Instynktowna ucieczka przed napastnikiem o większej sile.</summary>
    Fleeing
}
