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
        "Zbieraj surowce w ci¹gu dnia!",
        "Noc¹ potwory s¹ silniejsze!",
        "Rozmawiaj z NPC, aby zdobyæ nagrody!"
    };

    private string targetScene;
    private bool isLoadingComplete = false;

    private void Start()
    {
        // Pobierz nazwê sceny do za³adowania z PlayerPrefs
        targetScene = PlayerPrefs.GetString("SceneToLoad", "SampleScene");

        // Wybierz losow¹ poradê
        ShowRandomTip();

        // Rozpocznij ³adowanie
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
        
        // ETAP 1: Rozpocznij asynchroniczne ³adowanie sceny
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false; // Nie prze³¹czaj od razu

        // Status
        Debug.Log("£adowanie zasobów sceny...");

        // ETAP 2: Czekaj a¿ podstawowe assets siê za³aduj¹
        while (operation.progress < 0.9f)
        {
            float assetProgress = operation.progress / 0.9f;
            UpdateProgressUI(assetProgress, "Zasoby");
            yield return null;
        }

        // ETAP 3: Assets za³adowane, ale scena jeszcze nie aktywna
        Debug.Log("Inicjalizacja systemów gry...");
        yield return new WaitForSeconds(0.5f); // Ma³e opóŸnienie dla efektu

        // ETAP 4: Aktywuj scenê - TERAZ ³aduj¹ siê i wykonuj¹ skrypty
        operation.allowSceneActivation = true;

        // ETAP 5: Czekaj a¿ scena bêdzie fully loaded (w³¹cznie ze skryptami)
        yield return new WaitUntil(() => operation.isDone);

        // ETAP 6: Dodatkowe czekanie na inicjalizacjê skryptów
        Debug.Log("Finalizowanie...");
        yield return new WaitForSeconds(1f);

        // Teraz wszystkie skrypty (w tym GameState) s¹ za³adowane i gotowe
        Debug.Log("SCENA W PE£NI ZA£ADOWANA - WSZYSTKIE SKRYPTY GOTOWE");
    }
    private void UpdateProgressUI(float progress, string stage = "")
    {
        progressBar.value = progress;

        string stageText = string.IsNullOrEmpty(stage) ? "" : $" ({stage})";
        progressText.text = $"{progress * 100:0}%{stageText}";

        Debug.Log($"Progress: {progress * 100:0}% - {stage}");
    }

    private void Update()
    {
        // Opcjonalnie: mo¿esz dodaæ animacje podczas ³adowania
        if (!isLoadingComplete)
        {
            // Animacja ³adowania (np. obracaj¹ca siê ikona)
        }
    }
}
