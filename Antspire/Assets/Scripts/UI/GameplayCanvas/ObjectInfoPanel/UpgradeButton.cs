using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    Button button;
    [SerializeField] GameObject targetObject;
    public event System.Action<GameObject> OnUpgradeClicked;

    // Subskrypcja raz przy starcie - dodawanie listenera w Update() narastało
    // w nieskończoność (wyciek pamięci + tysiące wywołań jednego kliknięcia).
    void Start()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("UpgradeButton: brak komponentu Button na tym obiekcie.", this);
            return;
        }

        button.onClick.AddListener(OnUpgradeButtonClicked);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnUpgradeButtonClicked);
    }

    void OnUpgradeButtonClicked()
    {
        // Handle upgrade button click
        OnUpgradeClicked?.Invoke(gameObject);
    }
}
