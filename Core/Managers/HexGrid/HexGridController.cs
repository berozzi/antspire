using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using static UnityEditor.PlayerSettings;


public class HexGridController : MonoBehaviour
{
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