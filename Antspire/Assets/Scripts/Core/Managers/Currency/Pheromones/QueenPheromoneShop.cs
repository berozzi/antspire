using UnityEngine;

public class QueenPheromoneShop : MonoBehaviour, IClickable
{
    public void OnClick(){ Debug.Log("Queen shop clicked - state change should handle UI"); }
    public GameStates GetTargetState() => GameStates.QueenShop;
}
