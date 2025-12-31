using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{

    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text descriptionText;

    public void ShowInfoWithData(ClickableData data)
    {
        objectNameText.text = data.Name;
        descriptionText.text = data.Description;
    }
}
