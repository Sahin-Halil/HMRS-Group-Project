using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public bool isActivated = false;
    public Color inactiveColour = Color.grey;
    public Color activeColour = Color.green;

    private Renderer checkpointRenderer;
    private GameManager gameManager;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
        gameManager = Object.FindFirstObjectByType<GameManager>();
        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;

        if(gameManager != null)
        {
            gameManager.SetCheckpoint(transform.position);
        }

        UpdateVisuals();
        Debug.Log($"Checkpoint activated at {transform.position}");
    }

    void UpdateVisuals()
    {
        if(checkpointRenderer != null)
        {
            checkpointRenderer.material.color = isActivated ? activeColour : inactiveColour;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : Color.grey;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
