using UnityEngine;

public class ManualResourceAdding : MonoBehaviour
{
    private void Start()
    {
        Transform transform = this.transform;
        GameSave gameSave = GameSave.Instance;
        // Przyk³adowe dane zasobu do dodania
        ResourceData newResource = new ResourceData
        {
            id = System.Guid.NewGuid().ToString(),
            resourceName = "Wood",
            type = ResourceType.Wood,
            x = transform.position.x,
            y = transform.position.z,
            value = 1,
            isAvailable = true
        };
        // Dodaj zasób do GameSave
        gameSave.resources.Add(newResource);
        Debug.Log($"Added resource {newResource.resourceName} at ({newResource.x}, {newResource.y})");
    }

}
