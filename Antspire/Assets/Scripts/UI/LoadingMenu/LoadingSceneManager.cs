using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Settings")]
    [SerializeField]
    private string[] loadingTips = {
        "Zbieraj surowce w ci�gu dnia!",
        "Noc� potwory s� silniejsze!",
        "Rozmawiaj z NPC, aby zdoby� nagrody!"
    };

    private string targetScene;
    private bool isLoadingComplete = false;

    private void Start()
    {
        // Pobierz nazw� sceny do za�adowania z PlayerPrefs
        targetScene = PlayerPrefs.GetString("SceneToLoad", "SampleScene");

        // Wybierz losow� porad�
        ShowRandomTip();

        // Rozpocznij �adowanie
        StartCoroutine(LoadSceneAsync());
    }

    private void ShowRandomTip()
    {
        if (loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        
        // ETAP 1: Rozpocznij asynchroniczne �adowanie sceny
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        if (operation == null)
        {
            Debug.LogError($"LoadingSceneManager: nie mo�na za�adowa� sceny '{targetScene}'. " +
                           "Sprawd�, czy jest dodana w Build Settings.");
            yield break;
        }
        operation.allowSceneActivation = false; // Nie prze��czaj od razu

        // Status
        Debug.Log("�adowanie zasob�w sceny...");

        // ETAP 2: Czekaj a� podstawowe assets si� za�aduj�
        while (operation.progress < 0.9f)
        {
            float assetProgress = operation.progress / 0.9f;
            UpdateProgressUI(assetProgress, "Zasoby");
            yield return null;
        }

        // ETAP 3: Assets za�adowane, ale scena jeszcze nie aktywna
        Debug.Log("Inicjalizacja system�w gry...");
        // WaitForSecondsRealtime, bo WaitForSeconds stoi gdy Time.timeScale == 0
        // (gra zapauzowana) - �adowanie zawiesi�oby si� wtedy na zawsze.
        yield return new WaitForSecondsRealtime(0.5f); // Ma�e op�nienie dla efektu

        // ETAP 4: Aktywuj scen� - TERAZ �aduj� si� i wykonuj� skrypty
        operation.allowSceneActivation = true;

        // ETAP 5: Czekaj a� scena b�dzie fully loaded (w��cznie ze skryptami)
        yield return new WaitUntil(() => operation.isDone);

        // ETAP 6: Dodatkowe czekanie na inicjalizacj� skrypt�w
        Debug.Log("Finalizowanie...");
        yield return new WaitForSecondsRealtime(1f);

        isLoadingComplete = true;

        // Teraz wszystkie skrypty (w tym GameState) s� za�adowane i gotowe
        Debug.Log("SCENA W PE�NI ZA�ADOWANA - WSZYSTKIE SKRYPTY GOTOWE");
    }
    private void UpdateProgressUI(float progress, string stage = "")
    {
        progressBar.value = progress;

        string stageText = string.IsNullOrEmpty(stage) ? "" : $" ({stage})";
        progressText.text = $"{progress * 100:0}%{stageText}";
        // Bez Debug.Log - ta metoda jest wo�ana co klatk� podczas �adowania.
    }

    private void Update()
    {
        // Opcjonalnie: mo�esz doda� animacje podczas �adowania
        if (!isLoadingComplete)
        {
            // Animacja �adowania (np. obracaj�ca si� ikona)
        }
    }
}
