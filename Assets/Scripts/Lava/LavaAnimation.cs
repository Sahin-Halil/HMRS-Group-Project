using UnityEngine;

public class LavaAnimation : MonoBehaviour
{
    [Header("Flow Settings")]
    [SerializeField] private Vector2 flowDirection = new Vector2(0.1f, 0.05f);
    [SerializeField] private float flowSpeed = 1f;
    
    [Header("Material Settings")]
    [SerializeField] private Material lavaMaterial;
    private string texturePropertyName = "_MainTex";
    
    private Renderer meshRenderer;
    private Material instanceMaterial;
    private Vector2 offset;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        
        if (meshRenderer == null)
        {
            Debug.LogError("LavaAnimation: No Renderer component found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Create material instance to avoid modifying the shared material
        if (lavaMaterial != null)
        {
            instanceMaterial = new Material(lavaMaterial);
            meshRenderer.material = instanceMaterial;
        }
        else
        {
            instanceMaterial = meshRenderer.material;
        }

        offset = Vector2.zero;
    }

    private void Update()
    {
        if (instanceMaterial != null)
        {
            // Update offset based on time and flow direction
            offset += flowDirection * flowSpeed * Time.deltaTime;
            
            // Apply the offset to the material's texture
            instanceMaterial.SetTextureOffset(texturePropertyName, offset);
        }
    }

    private void OnDestroy()
    {
        // Clean up the material instance when destroyed
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}