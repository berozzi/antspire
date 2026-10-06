/// <summary>
/// Stany, w których może znajdować się mrówka-worker sterowana przez <see cref="AntWorkerAI"/>.
/// </summary>
public enum AntAIState
{
    Idle,
    Moving,
    Working,
    Exploring,
    Battling,
    Fleeing
}
