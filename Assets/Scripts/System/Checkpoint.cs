using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Checkpoint Settings
    public bool isActivated = false;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    public bool saveOnActivate = true;

    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;

        if (GameManager.Instance != null && saveOnActivate)
        {
            GameManager.Instance.SetCheckpoint(transform.position);
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = isActivated ? activeColor : inactiveColor;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}