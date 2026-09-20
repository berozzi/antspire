using UnityEngine;

public enum GameStates
{
    InGame,
    Paused,
    BuildMode,
    DestroyMode,
    TechTree,
    WarPanel,
    InStructureMenu,
    QueenShop
}

/// Odpowiada wyłącznie za stan gry (wartość + przejścia + eventy).
/// Całą orkiestracją gry zajął się GameManager.
public class GameState : MonoBehaviour
{
    public static GameStates CurrentGameState { get; private set; } = GameStates.InGame;

    // Eventy dla UI
    public event System.Action<bool> OnTechTreeToggled;
    public event System.Action<bool> OnWarPanelToggled;
    public event System.Action<bool> OnStructureMenuToggled;
    public event System.Action<GameStates, GameStates> OnStateChanged;

    public void SetGameState(GameStates newState)
    {
        if (CurrentGameState == newState) return;

        GameStates previousState = CurrentGameState;
        CurrentGameState = newState;

        HandlePanelToggleEvents(previousState, newState);
        OnStateChanged?.Invoke(previousState, newState);

        Debug.Log($"GameState changed: {previousState} -> {newState}");
    }

    public void TogglePanel(GameStates current)
    {
        SetGameState(CurrentGameState == current ? GameStates.InGame : current);
    }

    private void HandlePanelToggleEvents(GameStates previousState, GameStates newState)
    {
        if (previousState == GameStates.TechTree && newState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(false);
        else if (newState == GameStates.TechTree && previousState != GameStates.TechTree)
            OnTechTreeToggled?.Invoke(true);

        if (previousState == GameStates.WarPanel && newState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(false);
        else if (newState == GameStates.WarPanel && previousState != GameStates.WarPanel)
            OnWarPanelToggled?.Invoke(true);

        if (previousState == GameStates.InStructureMenu && newState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(false);
        else if (newState == GameStates.InStructureMenu && previousState != GameStates.InStructureMenu)
            OnStructureMenuToggled?.Invoke(true);
    }
}