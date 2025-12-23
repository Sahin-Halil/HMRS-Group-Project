using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoseEndingController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI statsText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 2f;
    public GameObject deathAnimation;

    [Header("Audio")]
    public AudioSource ambientAudio;
    public AudioClip deathSound;
    public AudioClip ambientDrone;

    private void Start()
    {
        SetupUI();
        StartCoroutine(PlayLoseSequence());
    }

    private void SetupUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private IEnumerator PlayLoseSequence()
    {
        // Play death sound
        if (ambientAudio != null && deathSound != null)
        {
            ambientAudio.PlayOneShot(deathSound);
        }

        // Show death animation
        if (deathAnimation != null)
        {
            deathAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        // Start ambient drone
        if (ambientAudio != null && ambientDrone != null)
        {
            ambientAudio.clip = ambientDrone;
            ambientAudio.loop = true;
            ambientAudio.Play();
        }

        // Fade in UI
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
        }

        // Display ending text
        DisplayLoseText();
        DisplayStats();
    }

    private void DisplayLoseText()
    {
        if (titleText != null)
        {
            titleText.text = "YOU LOSE";
        }

        if (storyText != null)
        {
            storyText.text = "";
        }
    }

    private void DisplayStats()
    {
        if (statsText != null && EndingManager.Instance != null)
        {
            int minutes = Mathf.FloorToInt(EndingManager.Instance.survivalTime / 60f);
            int seconds = Mathf.FloorToInt(EndingManager.Instance.survivalTime % 60f);

            statsText.text = $"<b>FINAL STATISTICS</b>\n\n" +
                           $"Ship Parts Collected: {EndingManager.Instance.shipPartsCollected}/5\n" +
                           $"Survival Time: {minutes:00}:{seconds:00}\n" +
                           $"Enemies Avoided: {EndingManager.Instance.enemiesAvoided}\n" +
                           $"Cause of Death: {(EndingManager.Instance.oxygenDepleted ? "Oxygen Depletion" : "Enemy Attack")}\n\n" +
                           $"<color=red>STATUS: DECEASED</color>";
        }
    }

    private void OnRetryClicked()
    {
        if (EndingManager.Instance != null)
        {
            EndingManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    private void OnMainMenuClicked()
    {
        if (EndingManager.Instance != null)
        {
            EndingManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
