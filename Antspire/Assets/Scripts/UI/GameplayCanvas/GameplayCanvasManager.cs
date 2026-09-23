using UnityEngine;

public class GameplayCanvasManager : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [Header("UI Panels")]
    [SerializeField] GameObject techTreePanel;
    [SerializeField] GameObject warPanel;
    [SerializeField] GameObject structureMenuPanel;
    [SerializeField] GameObject structureCommonMenu;
    [SerializeField] GameObject pheromoneShop;
    // [SerializeField] GameObject healthStructureMenu;
    [SerializeField] GameObject tunnelMenu;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.OnTechTreeToggled += OnTechTreeToggled;
            gameManager.OnWarPanelToggled += OnWarPanelToggled;
            gameManager.OnStructureMenuToggled += OnStructureMenuToggled;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTechTreeToggled -= OnTechTreeToggled;
            gameManager.OnWarPanelToggled -= OnWarPanelToggled;
            gameManager.OnStructureMenuToggled -= OnStructureMenuToggled;
        }
    }

    private void OnTechTreeToggled(bool isOpen)
    {
        techTreePanel.SetActive(isOpen);

        // Automatycznie zamknij inne panele
        if (isOpen && warPanel.activeSelf && pheromoneShop.activeSelf)
            warPanel.SetActive(false);
    }

    private void OnWarPanelToggled(bool isOpen)
    {
        warPanel.SetActive(isOpen);

        // Automatycznie zamknij inne panele
        if (isOpen && techTreePanel.activeSelf && pheromoneShop.activeSelf)
            techTreePanel.SetActive(false);
    }
    private void OnStructureMenuToggled(bool isOpen)
    {
        if (isOpen)
        {
            CloseAllPanels();
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
    public void TogglePheromoneShop(bool isOpen)
    {
        pheromoneShop.SetActive(isOpen);
        Debug.Log("Toggling Pheromone Shop Panel");
        // Implementacja dla panelu sklepu z feromonami
    }

    public void ToggleTunnelMenu()
    {
        tunnelMenu.SetActive(!tunnelMenu.activeSelf);
        Debug.Log("Toggling Tunnel Menu Panel");
        // Implementacja dla panelu tuneli
    }

    public void CloseAllPanels()
    {
        techTreePanel.SetActive(false);
        warPanel.SetActive(false);
        pheromoneShop.SetActive(false);
    }

    public void CloseAllSubpanels()
    {
        structureCommonMenu.SetActive(false);
        tunnelMenu.SetActive(false);
        // healthStructureMenu.SetActive(false);
        // Dodaj tutaj zamykanie innych podpaneli, je�li istniej�
    }
}
