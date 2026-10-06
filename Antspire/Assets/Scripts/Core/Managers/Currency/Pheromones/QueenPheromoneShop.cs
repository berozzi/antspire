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
    /// Opcjonalnie: skonfigurowana mrówka (NavMeshAgent + AntWorkerAI + AntThreatResponse).
    /// Prefab sam w sobie nie ma AI ani agenta, więc bez szablonu spawnione
    /// mrówki stałyby w miejscu. Gdy pole jest puste, szukamy takiej mrówki w scenie.
    /// </summary>
    [SerializeField] private AntWorkerAI antTemplate;

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

        // Najlepsza ścieżka: klon skonfigurowanej mrówki (agent + AI + walka).
        AntWorkerAI template = antTemplate != null ? antTemplate : FindAnyObjectByType<AntWorkerAI>();
        if (template != null)
        {
            Instantiate(template.gameObject, spawnPos, template.transform.rotation);
            Debug.Log("Robotnica zaspawnowana za darmo.");
            return;
        }

        // Fallback: sam prefab - dorzuci pełny zestaw AI, mrówka sama szuka rutyny (praca/transport/eksploracja).
        if (antWorkerPrefab == null)
        {
            Debug.LogError("QueenPheromoneShop: brak przypisanego prefabu mrówki ani szablonu w scenie.", this);
            return;
        }

        GameObject ant = Instantiate(antWorkerPrefab, spawnPos, Quaternion.identity);
        EnsureAntCanMove(ant);
        Debug.Log("Robotnica zaspawnowana za darmo (z prefabu - AI dodane w locie).");
    }

    /// <summary>Dodaje brakujące komponenty AI i ruchu, jeśli prefab ich nie ma.</summary>
    private static void EnsureAntCanMove(GameObject ant)
    {
        if (ant.GetComponent<NavMeshAgent>() == null)
        {
            NavMeshAgent agent = ant.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.stoppingDistance = 2f;
            agent.autoBraking = true;
        }

        // AntWorkerAI sam dociąga brakujące komponenty (NavMeshAgent, AntHealth, AntMover).
        if (ant.GetComponent<AntWorkerAI>() == null)
            ant.AddComponent<AntWorkerAI>();

        if (ant.GetComponent<AntThreatResponse>() == null)
            ant.AddComponent<AntThreatResponse>();
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