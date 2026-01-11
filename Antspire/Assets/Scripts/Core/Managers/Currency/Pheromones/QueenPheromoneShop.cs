using UnityEngine;
using UnityEngine.UI;

public class QueenPheromoneShop : MonoBehaviour, IClickable
{
    [SerializeField] private GameObject queenButton;
    private Button actualButton;
    [SerializeField] private GameObject antWorkerPrefab;
    [SerializeField] private Transform queenPosition;

    private void Awake()
    {
        if (queenButton != null)
        {
            actualButton = queenButton.GetComponent<Button>();
            if (actualButton != null)
            {
                actualButton.onClick.AddListener(BuyAnt);
            }
            else
            {
                Debug.LogError("Button component not found on Queen Button GameObject.");
            }
        }
        else
        {
            Debug.LogError("Queen Button is not assigned in the inspector. Trying to get component.");
        }
    }

    void BuyAnt()
    {
        Instantiate(antWorkerPrefab, new Vector3(queenPosition.position.x, 0, queenPosition.position.z - 2), Quaternion.identity);
        Debug.Log("Ant worker purchased!");
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
            Description = "Purchase ant workers using pheromones."
        };
    }
}
