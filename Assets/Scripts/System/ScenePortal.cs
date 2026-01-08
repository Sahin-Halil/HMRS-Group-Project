using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class ScenePortal : MonoBehaviour
{
    // Scene Settings
    public string targetSceneName = "Level2";
    public string portalName = "Ship Interior";

    // Requirements
    public bool requiresPuzzles = true;
    public int requiredPuzzlesCompleted = 4;

    // UI
    public TMP_Text promptText;
    public GameObject transitionPanel;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool playerInRange = false;
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
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryEnterPortal();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
            HidePrompt();
        }
    }

    bool CanUsePortal()
    {
        // Portal requires all key cards (outdoor)
        if (requiresPuzzles)
        {
            if (KeyCardManager.Instance == null) return false;
            return KeyCardManager.Instance.HasAllCards();
        }

        return true;
    }

    string GetRequirementMessage()
    {
        if (KeyCardManager.Instance == null) return "System Error";

        int collected = KeyCardManager.Instance.GetCollectedCards();
        int remaining = 4 - collected;

        return $"Airlock Locked! Collect {remaining} more key card(s) to enter.";
    }

    void TryEnterPortal()
    {
        if (!CanUsePortal()) return;

        StartCoroutine(LoadSceneWithFade());
    }

    IEnumerator LoadSceneWithFade()
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(FadeScreen(true));

        // Save checkpoint before transitioning
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCheckpoint();
        }

        SceneManager.LoadScene(targetSceneName);
    }

    IEnumerator FadeScreen(bool fadeOut)
    {
        if (fadeImage == null) yield break;

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one);
    }
}