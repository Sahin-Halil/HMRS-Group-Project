using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Checkpoint Settings
    public bool isActivated = false;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    public bool saveOnActivate = true;

    public GameObject checkpointMessageUI;
    public float messageDisplayTime = 2f;

    // Sound
    public AudioClip activationSound;
    private AudioSource audioSource;
    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        UpdateVisuals();

        if (checkpointMessageUI != null)
        {
            checkpointMessageUI.SetActive(false);
        }
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
            Vector3 spawnPosition = transform.position + Vector3.up * 1.5f;
            GameManager.Instance.SetCheckpoint(transform.position);
        }

        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Show message
        ShowCheckpointMessage();

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = isActivated ? activeColor : inactiveColor;
        }
    }

    void ShowCheckpointMessage()
    {
        if (checkpointMessageUI != null)
        {
            checkpointMessageUI.SetActive(true);
            Invoke(nameof(HideCheckpointMessage), messageDisplayTime);
        }
    }

    void HideCheckpointMessage()
    {
        if (checkpointMessageUI != null)
        {
            checkpointMessageUI.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}