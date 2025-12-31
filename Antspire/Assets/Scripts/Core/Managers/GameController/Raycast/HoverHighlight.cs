using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float emissionIntensity = 1.5f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void EnableHighlight()
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColor, highlightColor * emissionIntensity);
            r.SetPropertyBlock(propertyBlock);
        }
    }

    public void DisableHighlight()
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColor, Color.black);
            r.SetPropertyBlock(propertyBlock);
        }
    }
}
