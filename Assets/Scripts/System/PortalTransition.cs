// Player currently starts indoors for testing so once portal is used,
// player cannot re-enter until outdoor puzzles complete
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PortalTransition : MonoBehaviour
{
    // Portal Settings
    public Transform destinationPortal;
    public string portalName = "Airlock Entry";
    public bool isOutdoorPortal = true;

    // Access Requirements
    public bool requiresPuzzleCompletion = true;
    public int requiredPuzzlesCompleted = 2; // Engine + Cockpit

    // UI
    public TMP_Text promptText;
    public GameObject transitionPanel;
    public Image fadeImage;
    public float fadeDuration = 1f;

    // TODO: Audio
    public AudioClip portalSound;

    private bool playerInRange = false;
    private Transform player;
    private CharacterController playerController;
    private bool isTransitioning = false;

    void Start()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && !isTransitioning)
        {
            // Check for E key press
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryUsePortal();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerController = player.GetComponent<CharacterController>();
            playerInRange = true;

            if (CanUsePortal())
            {
                ShowPrompt($"Press E to enter {portalName}");
            }
            else
            {
                ShowPrompt(GetRequirementMessage());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            HidePrompt();
        }
    }

    bool CanUsePortal()
    {
        if (!requiresPuzzleCompletion)
        {
            return true;
        }

        // Only outdoor portal requires puzzles completed
        if (!isOutdoorPortal)
        {
            return true;
        }

        // Check if required puzzles are completed
        if (ShipPartManager.Instance == null)
        {
            return false;
        }

        int completedCount = 0;
        if (ShipPartManager.Instance.enginePuzzleCompleted) completedCount++;
        if (ShipPartManager.Instance.cockpitPuzzleCompleted) completedCount++;

        return completedCount >= requiredPuzzlesCompleted;
    }

    string GetRequirementMessage()
    {
        if (!isOutdoorPortal)
        {
            return "Press E to exit ship";
        }

        if (ShipPartManager.Instance == null)
        {
            return "System Error";
        }

        int completedCount = 0;
        if (ShipPartManager.Instance.enginePuzzleCompleted) completedCount++;
        if (ShipPartManager.Instance.cockpitPuzzleCompleted) completedCount++;

        return $"Airlock Locked! Complete {requiredPuzzlesCompleted - completedCount} more outdoor systems first.";
    }

    void TryUsePortal()
    {
        if (!CanUsePortal())
        {
            return;
        }

        if (destinationPortal == null)
        {
            return;
        }

        StartCoroutine(TransitionToDestination());
    }

    System.Collections.IEnumerator TransitionToDestination()
    {
        isTransitioning = true;

        // TODO: play sound
        if (portalSound != null)
        {
            AudioSource.PlayClipAtPoint(portalSound, transform.position);
        }

        yield return StartCoroutine(FadeScreen(true));

        TeleportPlayer();
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeScreen(false));

        isTransitioning = false;
    }

    void TeleportPlayer()
    {
        if (player == null || destinationPortal == null) return;

        // Disable character controller to teleport
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Move player to destination
        player.position = destinationPortal.position + destinationPortal.forward * 2f;
        player.rotation = destinationPortal.rotation;

        // Re-enable character controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Save checkpoint at new location
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(player.position);
        }
    }

    System.Collections.IEnumerator FadeScreen(bool fadeOut)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = fadeOut ? (elapsed / fadeDuration) : (1f - elapsed / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = fadeOut ? 1f : 0f;
        fadeImage.color = color;

        if (!fadeOut && transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }
    }

    void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isOutdoorPortal ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 2f);

        if (destinationPortal != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, destinationPortal.position);
        }
    }
}