using UnityEngine;

public class GameplayCanvasManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] GameObject techTreePanel;
    [SerializeField] GameObject warPanel;
    [SerializeField] GameState gameState;

    private void Awake()
    {
        gameState = FindAnyObjectByType<GameState>();

        if (gameState != null)
        {
            gameState.OnTechTreeToggled += OnTechTreeToggled;
            gameState.OnWarPanelToggled += OnWarPanelToggled;
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnTechTreeToggled -= OnTechTreeToggled;
            gameState.OnWarPanelToggled -= OnWarPanelToggled;
        }
    }

    private void OnTechTreeToggled(bool isOpen)
    {
        techTreePanel.SetActive(isOpen);

        // Automatycznie zamknij inne panele
        if (isOpen && warPanel.activeSelf)
            warPanel.SetActive(false);
    }

    private void OnWarPanelToggled(bool isOpen)
    {
        warPanel.SetActive(isOpen);

        // Automatycznie zamknij inne panele
        if (isOpen && techTreePanel.activeSelf)
            techTreePanel.SetActive(false);
    }
}
