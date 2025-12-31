using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private MainMenuManager MainMenuManager;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button loadButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    //[SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Podpiêcie eventów
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        loadButton.onClick.AddListener(OnLoadGame);

        ShowMainPanel();
    }

    private void OnPlayClicked()
    {
        MainMenuManager.PlayGame();
    }

    private void OnSettingsClicked()
    {
        MainMenuManager.OpenSettings();
    }

    private void OnQuitClicked()
    {
        MainMenuManager.QuitGame();
    }
    private void OnLoadGame()
    {
        MainMenuManager.LoadGame();
    }

    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        //settingsPanel.SetActive(false);
    }

    private void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        //settingsPanel.SetActive(true);
    }
}
