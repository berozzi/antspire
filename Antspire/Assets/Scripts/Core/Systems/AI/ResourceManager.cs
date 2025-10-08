using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private GameObject resourcePrefabs;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeResources();
    }

    void InitializeResources()
    {
        // Jeœli nie ma zasobów w save'ie, wygeneruj nowe
        if (GameSave.Instance.resources.Count == 0)
        {
            GenerateInitialResources();
        }
        else
        {
            SpawnSavedResources();
        }
    }
    // to dzia³a
    void GenerateInitialResources()
    {
        GameSave gameSave = GameSave.Instance;
        if (gameSave.resources == null)
        {
            gameSave.resources = new List<ResourceData>();
            Debug.Log("Resources list was null, initializing a new list.");
        }
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = GetRandomMapPosition();
            ResourceType randomType = (ResourceType)Random.Range(0, 4);

            ResourceData resourceData = new ResourceData()
            {
                id = System.Guid.NewGuid().ToString(),
                resourceName = randomType.ToString(),
                type = randomType,
                x = randomPos.x,
                y = randomPos.z,
                value = 1,
                isAvailable = true
            };

            // Dodaj do save'a
            //gameSave.resources.Add(resourceData);
            //Debug.Log($"Created resource {resourceData.resourceName} at ({resourceData.x}, {resourceData.y}, also the type is: {resourceData.type})");

            SpawnResourceFromData(resourceData);
        }
    }

    void SpawnSavedResources()
    {
        foreach (ResourceData resourceData in GameSave.Instance.resources)
        {
            if (resourceData.isAvailable)
            {
                SpawnResourceFromData(resourceData);
            }
        }
    }

    void SpawnResourceFromData(ResourceData resourceData)
    {
        //GameObject resourcePrefab = resourcePrefabs.;
        GameObject resourceObj = Instantiate(resourcePrefabs, new Vector3(resourceData.x, resourceData.y, 0), Quaternion.identity);

        // Przypisz dane do obiektu
        ResourceComponent resourceComp = resourceObj.GetComponent<ResourceComponent>();
        if (resourceComp != null)
        {
            resourceComp.Initialize(resourceData);
        }
    }

    Vector3 GetRandomMapPosition()
    {
        // Dostosuj do rozmiaru twojej mapy
        return new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
    }
}
