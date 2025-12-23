using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinEndingController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI statsText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 2f;
    public GameObject shipRepairAnimation;

    [Header("Audio")]
    public AudioSource victoryMusic;
    public AudioClip victorySound;

    private void Start()
    {
        SetupUI();
        StartCoroutine(PlayWinSequence());
    }

    private void SetupUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private IEnumerator PlayWinSequence()
    {
        // Play victory sound
        if (victoryMusic != null && victorySound != null)
        {
            victoryMusic.PlayOneShot(victorySound);
        }

        // Show ship repair animation
        if (shipRepairAnimation != null)
        {
            shipRepairAnimation.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

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
        DisplayWinText();
        DisplayStats();
    }

    private void DisplayWinText()
    {
        if (titleText != null)
        {
            titleText.text = "YOU WIN";
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

            statsText.text = $"<b>MISSION STATISTICS</b>\n\n" +
                           $"Ship Parts Collected: {EndingManager.Instance.shipPartsCollected}/5\n" +
                           $"Survival Time: {minutes:00}:{seconds:00}\n" +
                           $"Enemies Avoided: {EndingManager.Instance.enemiesAvoided}\n" +
                           $"Remaining Oxygen: {EndingManager.Instance.remainingOxygen:F1}%\n\n" +
                           $"<color=green>STATUS: ESCAPED</color>";
        }
    }

    private void OnRestartClicked()
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
