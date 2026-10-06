using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Królowa - testowy punkt spawnu mrówek robotnic.
/// Spawn jest za darmo: feromony nie są tu walutą, służą jako "paliwo"
/// (odblokowanie techtree, budowa szybszych dróg i inne zadania).
/// </summary>
public class QueenPheromoneShop : MonoBehaviour, IClickable
{
    [SerializeField] private GameObject queenButton;
    private Button actualButton;
    [SerializeField] private GameObject antWorkerPrefab;
    [SerializeField] private Transform queenPosition;

    /// <summary>
    /// Opcjonalnie: skonfigurowana mrówka (NavMeshAgent + AntBasicAI z celami).
    /// Prefab sam w sobie nie ma AI ani agenta, więc bez szablonu spawnione
    /// mrówki stałyby w miejscu. Gdy pole jest puste, szukamy takiej mrówki w scenie.
    /// </summary>
    [SerializeField] private AntBasicAI antTemplate;

    private void Awake()
    {
        if (queenButton != null)
            actualButton = queenButton.GetComponent<Button>();

        if (actualButton == null)
            actualButton = GetComponentInChildren<Button>();

        if (actualButton != null)
            actualButton.onClick.AddListener(SpawnAntWorker);
        else
            Debug.LogError("Button component not found on Queen Button GameObject.");
    }

    /// <summary>Spawnuje robotnicę za darmo (bez kosztu feromonów).</summary>
    public void SpawnAntWorker()
    {
        if (queenPosition == null)
        {
            Debug.LogError("QueenPheromoneShop: brak przypisanej pozycji królowej.", this);
            return;
        }

        Vector3 spawnPos = new Vector3(queenPosition.position.x, 0f, queenPosition.position.z - 2f);

        // Najlepsza ścieżka: klon skonfigurowanej mrówki (agent + AI + cele).
        AntBasicAI template = antTemplate != null ? antTemplate : FindAnyObjectByType<AntBasicAI>();
        if (template != null)
        {
            Instantiate(template.gameObject, spawnPos, template.transform.rotation);
            Debug.Log("Robotnica zaspawnowana za darmo.");
            return;
        }

        // Fallback: sam prefab - dołoży agenta, ale bez celów mrówka nie ruszy.
        if (antWorkerPrefab == null)
        {
            Debug.LogError("QueenPheromoneShop: brak przypisanego prefabu mrówki ani szablonu w scenie.", this);
            return;
        }

        GameObject ant = Instantiate(antWorkerPrefab, spawnPos, Quaternion.identity);
        EnsureAntCanMove(ant);
        Debug.Log("Robotnica zaspawnowana za darmo (brak szablonu AI w scenie - mrówka nie będzie się poruszać).");
    }

    /// <summary>Dodaje brakujące komponenty ruchu, jeśli prefab ich nie ma.</summary>
    private static void EnsureAntCanMove(GameObject ant)
    {
        if (ant.GetComponent<NavMeshAgent>() == null)
        {
            NavMeshAgent agent = ant.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.stoppingDistance = 2f;
            agent.autoBraking = true;
        }

        if (ant.GetComponent<AntBasicAI>() == null)
            ant.AddComponent<AntBasicAI>(); // bez celów - zaloguje błąd i stanie w miejscu
    }

    public GameStates GetTargetState() => GameStates.QueenShop;

    public void OnClick()
    {
        Debug.Log("Queen Pheromone Shop clicked.");
    }

    public ClickableData GetClickableData()
    {
        return new ClickableData
        {
            Name = "Queen Pheromone Shop",
            Description = "Spawns an ant worker for free."
        };
    }
}
