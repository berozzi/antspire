using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using static UnityEditor.PlayerSettings;

//[ExecuteInEditMode]
public class HexGridController : MonoBehaviour
{
    public bool showGridInSceneView = true;

#if UNITY_EDITOR
    private void OnEnable()
    {
        // Rejestruj siê do odœwie¿ania Scene View
        UnityEditor.EditorApplication.update += RefreshSceneView;
    }

    private void OnDisable()
    {
        // Wyrejestruj siê
        UnityEditor.EditorApplication.update -= RefreshSceneView;
    }

    private void RefreshSceneView()
    {
        if (showGridInSceneView)
        {
            // Wymusza odœwie¿enie Scene View
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif // UNITY_EDITOR - ten ca³y kod odpowiada za uruchamianie HexGrida w edytorze Unity
    [Header("Grid Settings")]
    public GameObject hexPrefab;
    public GameObject hexPrefab2;
    // wysokoœæ mapy w hexach
    void Start()
    {
        var generate = gameObject.AddComponent<GenerateGrid>();
        generate.Generate(hexPrefab, hexPrefab2);
    }
}