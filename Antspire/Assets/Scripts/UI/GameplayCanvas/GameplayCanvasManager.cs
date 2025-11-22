using UnityEngine;

public class GameplayCanvasManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] GameObject techTreePanel;
    [SerializeField] GameObject warPanel;
    [SerializeField] GameState gameState;
    [SerializeField] GameObject structureMenuPanel;
    [SerializeField] GameObject structureCommonMenu;

    private void Awake()
    {
        gameState = FindAnyObjectByType<GameState>();

        if (gameState != null)
        {
            gameState.OnTechTreeToggled += OnTechTreeToggled;
            gameState.OnWarPanelToggled += OnWarPanelToggled;
            gameState.OnStructureMenuToggled += OnStructureMenuToggled;
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnTechTreeToggled -= OnTechTreeToggled;
            gameState.OnWarPanelToggled -= OnWarPanelToggled;
            gameState.OnStructureMenuToggled -= OnStructureMenuToggled;
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
    private void OnStructureMenuToggled(bool isOpen)
    {
        if (isOpen)
        {
            structureMenuPanel.SetActive(true);
        }
        else
        {
            structureMenuPanel.SetActive(false);
            CloseAllSubpanels();
        }
    }

    public void ToggleCommonStructures()
    {
        structureCommonMenu.SetActive(!structureCommonMenu.activeSelf);
    }
    public void ToggleHealthStructures()
    {
        Debug.Log("Toggling Health Structures Panel");
        // Implementacja dla panelu struktur zdrowotnych
    }

    public void CloseAllPanels()
    {
        techTreePanel.SetActive(false);
        warPanel.SetActive(false);
    }

    public void CloseAllSubpanels()
    {
        structureCommonMenu.SetActive(false);
        // Dodaj tutaj zamykanie innych podpaneli, jeœli istniej¹
    }
}
