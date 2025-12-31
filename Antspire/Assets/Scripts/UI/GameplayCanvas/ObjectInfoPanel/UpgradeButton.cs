using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    Button button;
    [SerializeField] GameObject targetObject;
    public event System.Action<GameObject> OnUpgradeClicked;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        button.onClick.AddListener(OnUpgradeButtonClicked);
    }

    void OnUpgradeButtonClicked()
    {
        // Handle upgrade button click
        OnUpgradeClicked?.Invoke(gameObject);
    }
}
