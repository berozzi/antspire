using UnityEngine;

public class AntScanner : MonoBehaviour
{
    void Start()
    {
        var allAnts = Object.FindObjectsByType<ClickableAnt>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"🔍 Znaleziono {allAnts.Length} mrówek w scenie:");

        foreach (var ant in allAnts)
        {
            Debug.Log($"🟢 Obiekt: {ant.gameObject.name}, antName: {ant.antName}");
        }
    }
}


