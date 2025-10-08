using UnityEngine;

public class ResourceComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ResourceData resourceData;

    public void Initialize(ResourceData data)
    {
        resourceData = data;
        transform.position = new Vector3(data.x, data.y, 0);
    }
}
