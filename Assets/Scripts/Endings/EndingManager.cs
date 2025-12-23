using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    [Header("Ending Data")]
    public int shipPartsCollected = 0;
    public float survivalTime = 0f;
    public int enemiesAvoided = 0;
    public float remainingOxygen = 0f;
    public bool oxygenDepleted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetEndingData(int parts, float time, int enemies, float oxygen, bool oxygenDeath)
    {
        shipPartsCollected = parts;
        survivalTime = time;
        enemiesAvoided = enemies;
        remainingOxygen = oxygen;
        oxygenDepleted = oxygenDeath;
    }

    public void LoadWinEnding()
    {
        StartCoroutine(LoadEndingScene("WinEndingScene"));
    }

    public void LoadLoseEnding()
    {
        StartCoroutine(LoadEndingScene("LoseEndingScene"));
    }

    private IEnumerator LoadEndingScene(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        // Reset data
        shipPartsCollected = 0;
        survivalTime = 0f;
        enemiesAvoided = 0;
        remainingOxygen = 0f;
        oxygenDepleted = false;
        
        SceneManager.LoadScene("GameScene"); // Replace with your main game scene name
    }
}
